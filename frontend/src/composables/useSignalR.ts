import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ref } from 'vue'

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5160'

export function useSignalR() {
  const connection = ref<HubConnection | null>(null)
  const isConnected = ref(false)

  async function connect(visitId?: string) {
    if (connection.value) {
      await connection.value.stop()
    }

    connection.value = new HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/visit`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build()

    connection.value.onreconnecting(() => {
      isConnected.value = false
    })

    connection.value.onreconnected(() => {
      isConnected.value = true
      if (visitId) {
        connection.value?.invoke('JoinVisitGroup', visitId)
      }
    })

    connection.value.onclose(() => {
      isConnected.value = false
    })

    await connection.value.start()
    isConnected.value = true

    if (visitId) {
      await connection.value.invoke('JoinVisitGroup', visitId)
    }
  }

  function disconnect() {
    connection.value?.stop()
    connection.value = null
    isConnected.value = false
  }

  function onVisitStepChanged(callback: (data: { visitId: string; step: string; stepName: string }) => void) {
    connection.value?.on('VisitStepChanged', callback)
  }

  function onQueueUpdated(callback: (data: { clinicId: string; queueType: string }) => void) {
    connection.value?.on('QueueUpdated', callback)
  }

  return { connection, isConnected, connect, disconnect, onVisitStepChanged, onQueueUpdated }
}
