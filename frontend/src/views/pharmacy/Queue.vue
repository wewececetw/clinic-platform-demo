<template>
  <div class="pharmacy-queue">
    <header class="page-header">
      <h1>藥劑師配藥</h1>
    </header>

    <section class="card">
      <h2>待配藥列表</h2>
      <p v-if="loading" class="loading-text">載入中...</p>
      <p v-else-if="queue.length === 0" class="empty-text">目前無待配藥處方</p>
      <ul v-else class="queue-list">
        <li v-for="item in queue" :key="item.prescriptionId" class="queue-item">
          <div class="patient-info">
            <span class="patient-name">{{ item.patientName }}</span>
            <span class="rx-status" :class="`status-${item.status.toLowerCase()}`">
              {{ statusLabel(item.status) }}
            </span>
          </div>
          <div class="actions">
            <button
              v-if="item.status === 'Pending'"
              class="btn btn-primary"
              :disabled="processing === item.prescriptionId"
              @click="handleStartDispense(item.prescriptionId)"
            >
              {{ processing === item.prescriptionId ? '處理中...' : '開始配藥' }}
            </button>
            <button
              v-if="item.status === 'Dispensing'"
              class="btn btn-success"
              :disabled="processing === item.prescriptionId"
              @click="handleCompleteDispense(item.prescriptionId)"
            >
              {{ processing === item.prescriptionId ? '處理中...' : '配藥完成' }}
            </button>
          </div>
        </li>
      </ul>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { getPharmacyQueue, startDispense, completeDispense } from '@/api/client'

interface PharmacyItem {
  prescriptionId: string
  visitId: string
  patientName: string
  status: string
}

const queue = ref<PharmacyItem[]>([])
const loading = ref(true)
const processing = ref<string | null>(null)

let timer: ReturnType<typeof setInterval> | null = null

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    Pending: '待配藥',
    Dispensing: '配藥中',
    Completed: '已完成',
  }
  return map[status] || status
}

async function loadQueue() {
  try {
    queue.value = await getPharmacyQueue()
  } catch (e: any) {
    console.error('載入配藥佇列失敗', e)
  } finally {
    loading.value = false
  }
}

async function handleStartDispense(prescriptionId: string) {
  processing.value = prescriptionId
  try {
    await startDispense(prescriptionId)
    await loadQueue()
  } catch (e: any) {
    alert(`開始配藥失敗：${e.message}`)
  } finally {
    processing.value = null
  }
}

async function handleCompleteDispense(prescriptionId: string) {
  processing.value = prescriptionId
  try {
    await completeDispense(prescriptionId)
    await loadQueue()
  } catch (e: any) {
    alert(`配藥完成失敗：${e.message}`)
  } finally {
    processing.value = null
  }
}

onMounted(() => {
  loadQueue()
  timer = setInterval(loadQueue, 5000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<style scoped>
.pharmacy-queue {
  max-width: 800px;
  margin: 0 auto;
  padding: 16px;
}

.page-header h1 {
  font-size: 1.5rem;
  margin-bottom: 16px;
  color: #1a1a2e;
}

.card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.card h2 {
  font-size: 1.1rem;
  margin-bottom: 12px;
  color: #333;
}

.loading-text,
.empty-text {
  color: #999;
  text-align: center;
  padding: 24px 0;
}

.queue-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.queue-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 12px;
  border-bottom: 1px solid #f0f0f0;
  gap: 12px;
  flex-wrap: wrap;
}

.queue-item:last-child {
  border-bottom: none;
}

.patient-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.patient-name {
  font-weight: 600;
  font-size: 1.05rem;
  color: #1a1a2e;
}

.rx-status {
  font-size: 0.8rem;
  padding: 2px 10px;
  border-radius: 12px;
  font-weight: 500;
}

.status-pending {
  background: #fef3c7;
  color: #92400e;
}

.status-dispensing {
  background: #dbeafe;
  color: #1e40af;
}

.status-completed {
  background: #dcfce7;
  color: #166534;
}

.actions {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  cursor: pointer;
  white-space: nowrap;
  transition: opacity 0.2s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #2563eb;
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #1d4ed8;
}

.btn-success {
  background: #16a34a;
  color: #fff;
}

.btn-success:hover:not(:disabled) {
  background: #15803d;
}
</style>
