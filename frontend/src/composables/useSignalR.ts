import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ref } from 'vue'

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5160'

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting'

export interface QueueUpdatedPayload {
  clinicId: string
  queueType: string
  changeType: string
  timestamp: string
}

export interface VisitStepChangedPayload {
  visitId: string
  newStep: string
  stepName: string
  timestamp: string
}

export interface PatientCalledPayload {
  visitId: string
  roomNumber: string
  calledAt: string
}

export function useSignalR() {
  const connection = ref<HubConnection | null>(null)
  const state = ref<ConnectionState>('disconnected')
  const isConnected = ref(false)
  const reconnectedCallbacks: Array<() => void | Promise<void>> = []

  async function connect(visitId?: string, clinicId?: string) {
    if (connection.value) {
      await connection.value.stop()
    }

    state.value = 'connecting'
    isConnected.value = false

    connection.value = new HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/visit`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build()

    connection.value.onreconnecting(() => {
      state.value = 'reconnecting'
      isConnected.value = false
    })

    connection.value.onreconnected(async () => {
      state.value = 'connected'
      isConnected.value = true
      // 重連後重新加入原先的 group
      if (visitId) {
        await connection.value?.invoke('JoinVisitGroup', visitId)
      }
      if (clinicId) {
        await connection.value?.invoke('JoinClinicGroup', clinicId)
      }
      // 觸發所有註冊的補拉 callback
      for (const cb of reconnectedCallbacks) {
        try {
          await cb()
        } catch (err) {
          console.warn('重連補拉 callback 失敗', err)
        }
      }
    })

    connection.value.onclose(() => {
      state.value = 'disconnected'
      isConnected.value = false
    })

    await connection.value.start()
    state.value = 'connected'
    isConnected.value = true

    if (visitId) {
      await connection.value.invoke('JoinVisitGroup', visitId)
    }
    if (clinicId) {
      await connection.value.invoke('JoinClinicGroup', clinicId)
    }
  }

  function disconnect() {
    connection.value?.stop()
    connection.value = null
    state.value = 'disconnected'
    isConnected.value = false
    reconnectedCallbacks.length = 0
  }

  function onVisitStepChanged(callback: (data: VisitStepChangedPayload) => void) {
    connection.value?.on('VisitStepChanged', callback)
  }

  function onQueueUpdated(callback: (data: QueueUpdatedPayload) => void) {
    connection.value?.on('QueueUpdated', callback)
  }

  function onPatientCalled(callback: (data: PatientCalledPayload) => void) {
    connection.value?.on('PatientCalled', callback)
  }

  function onReconnected(callback: () => void | Promise<void>) {
    reconnectedCallbacks.push(callback)
  }

  return {
    connection,
    state,
    isConnected,
    connect,
    disconnect,
    onVisitStepChanged,
    onQueueUpdated,
    onPatientCalled,
    onReconnected,
  }
}
