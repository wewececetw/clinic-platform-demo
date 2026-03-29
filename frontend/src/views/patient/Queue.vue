<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useVisitStore } from '@/stores/visit'
import { useSignalR } from '@/composables/useSignalR'
import { getVisitStatus, getQueuePosition } from '@/api/client'

const router = useRouter()
const store = useVisitStore()
const { isConnected, connect, disconnect, onVisitStepChanged, onQueueUpdated } = useSignalR()

const CLINIC_ID = import.meta.env.VITE_CLINIC_ID || 'demo-clinic-001'

const stepName = ref('')
const status = ref('')
const position = ref<number | null>(null)
const total = ref<number | null>(null)
const loading = ref(true)
const error = ref('')

async function fetchStatus() {
  if (!store.currentVisitId) return
  try {
    const res = await getVisitStatus(store.currentVisitId)
    store.updateStep(res.currentStep)
    stepName.value = res.stepName
    status.value = res.status
  } catch (e: any) {
    error.value = e.message || '無法取得門診狀態'
  }
}

async function fetchPosition() {
  if (!store.currentVisitId) return
  try {
    const res = await getQueuePosition(CLINIC_ID, store.currentVisitId)
    position.value = res.position
    total.value = res.total
    store.updatePosition(res.position)
  } catch {
    // 位置查詢失敗不阻塞頁面
  }
}

onMounted(async () => {
  if (!store.currentVisitId) {
    router.replace('/')
    return
  }

  await Promise.all([fetchStatus(), fetchPosition()])
  loading.value = false

  // 建立 SignalR 連線
  try {
    await connect(store.currentVisitId)
    store.setConnected(true)

    onVisitStepChanged((data) => {
      store.updateStep(data.step)
      stepName.value = data.stepName
      fetchStatus()
    })

    onQueueUpdated(() => {
      fetchPosition()
    })
  } catch {
    store.setConnected(false)
  }
})

onUnmounted(() => {
  disconnect()
  store.setConnected(false)
})
</script>

<template>
  <div class="queue-page">
    <h1 class="title">候診狀態</h1>

    <!-- 連線狀態指示燈 -->
    <div class="connection-status">
      <span :class="['dot', isConnected ? 'connected' : 'disconnected']"></span>
      <span class="connection-text">{{ isConnected ? '即時連線中' : '連線中斷' }}</span>
    </div>

    <div v-if="loading" class="loading">載入中...</div>

    <template v-else-if="!error">
      <!-- 看診號碼 -->
      <div class="number-card">
        <p class="number-label">您的看診號碼</p>
        <p class="number-value">{{ store.queueNumber }}</p>
      </div>

      <!-- 目前步驟 -->
      <div class="info-card">
        <p class="info-label">目前步驟</p>
        <p class="info-value">{{ stepName || store.currentStep }}</p>
      </div>

      <!-- 候診位置 -->
      <div v-if="position !== null" class="info-card">
        <p class="info-label">前面還有</p>
        <p class="position-value">
          <span class="position-number">{{ position }}</span>
          <span class="position-unit">人</span>
        </p>
      </div>

      <!-- 狀態 -->
      <div v-if="status" class="info-card">
        <p class="info-label">狀態</p>
        <p class="info-value">{{ status }}</p>
      </div>
    </template>

    <div v-else class="error">{{ error }}</div>

    <button class="btn secondary" @click="router.replace('/')">返回報到頁</button>
  </div>
</template>

<style scoped>
.queue-page {
  max-width: 420px;
  margin: 0 auto;
  padding: 24px 16px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.title {
  text-align: center;
  font-size: 24px;
  margin-bottom: 8px;
  color: #1a1a1a;
}

.connection-status {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  margin-bottom: 24px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.dot.connected {
  background: #34a853;
  box-shadow: 0 0 4px rgba(52, 168, 83, 0.5);
}

.dot.disconnected {
  background: #d93025;
  box-shadow: 0 0 4px rgba(217, 48, 37, 0.5);
}

.connection-text {
  font-size: 12px;
  color: #888;
}

.loading {
  text-align: center;
  color: #666;
  padding: 40px 0;
  font-size: 16px;
}

.number-card {
  background: #1a73e8;
  color: #fff;
  border-radius: 16px;
  padding: 24px;
  text-align: center;
  margin-bottom: 16px;
}

.number-label {
  font-size: 14px;
  opacity: 0.9;
  margin: 0 0 8px;
}

.number-value {
  font-size: 72px;
  font-weight: 700;
  margin: 0;
  line-height: 1;
}

.info-card {
  background: #f8f9fa;
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  padding: 16px 20px;
  margin-bottom: 12px;
}

.info-label {
  font-size: 13px;
  color: #888;
  margin: 0 0 4px;
}

.info-value {
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.position-value {
  margin: 0;
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.position-number {
  font-size: 36px;
  font-weight: 700;
  color: #e8710a;
}

.position-unit {
  font-size: 18px;
  color: #666;
}

.error {
  color: #d93025;
  font-size: 14px;
  padding: 12px;
  background: #fce8e6;
  border-radius: 8px;
  margin-bottom: 16px;
}

.btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  margin-top: 20px;
  transition: all 0.2s;
}

.btn.secondary {
  background: #f0f0f0;
  color: #333;
}

.btn.secondary:hover {
  background: #e0e0e0;
}
</style>
