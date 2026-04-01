<template>
  <div class="nurse-dashboard">
    <header class="page-header">
      <h1>護理師儀表板</h1>
    </header>

    <div class="dashboard-layout">
      <!-- 左側：候診佇列 -->
      <section class="queue-panel">
        <h2>候診佇列</h2>
        <p v-if="loading" class="loading-text">載入中...</p>
        <p v-else-if="queue.length === 0" class="empty-text">目前無候診病患</p>
        <ul v-else class="queue-list">
          <li v-for="item in queue" :key="item.visitId" class="queue-item" :class="`status-${item.status.toLowerCase()}`">
            <div class="queue-info">
              <span class="queue-number">{{ item.queueNumber }}</span>
              <div class="patient-detail">
                <span class="patient-name">{{ item.patientName }}</span>
                <span class="patient-meta">
                  優先度：{{ item.priority }} ｜ 狀態：{{ item.status }}
                </span>
              </div>
            </div>
            <button class="btn btn-skip" @click="handleSkip(item.visitId)">過號</button>
          </li>
        </ul>
      </section>

      <!-- 右側 -->
      <aside class="action-panel">
        <!-- AI 指令 -->
        <section class="action-card">
          <h2>AI 指令</h2>
          <CommandInput
            :clinic-id="CLINIC_ID"
            :user-id="USER_ID"
            role="Nurse"
            placeholder="輸入指令：叫下一位、3號過號、還有幾位候診..."
            @executed="loadQueue"
          />
        </section>

        <!-- 叫下一號 -->
        <section class="action-card">
          <h2>叫號</h2>
          <button class="btn btn-primary btn-large" :disabled="calling" @click="handleCallNext">
            {{ calling ? '叫號中...' : '叫下一號' }}
          </button>
        </section>

        <!-- 手動報到 -->
        <section class="action-card">
          <h2>手動報到</h2>
          <form @submit.prevent="handleManualCheckIn" class="checkin-form">
            <div class="form-group">
              <label for="phone">手機號碼</label>
              <input id="phone" v-model="checkinForm.phone" type="tel" placeholder="0912345678" />
            </div>
            <div class="form-group">
              <label for="fullName">姓名</label>
              <input id="fullName" v-model="checkinForm.fullName" type="text" placeholder="病患姓名" />
            </div>
            <button class="btn btn-primary" type="submit" :disabled="checkingIn">
              {{ checkingIn ? '報到中...' : '報到' }}
            </button>
          </form>
        </section>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { getQueue, callNext, skipVisit, manualCheckIn } from '@/api/client'
import CommandInput from '@/components/CommandInput.vue'

const CLINIC_ID = '10000000-0000-0000-0000-000000000001'
const USER_ID = '20000000-0000-0000-0000-000000000001'

const queue = ref<Array<{ visitId: string; queueNumber: number; patientName: string; priority: number; status: string }>>([])
const loading = ref(true)
const calling = ref(false)
const checkingIn = ref(false)
const checkinForm = ref({ phone: '', fullName: '' })

let timer: ReturnType<typeof setInterval> | null = null

async function loadQueue() {
  try {
    queue.value = await getQueue(CLINIC_ID, 'Consulting')
  } catch (e: any) {
    console.error('載入佇列失敗', e)
  } finally {
    loading.value = false
  }
}

async function handleCallNext() {
  calling.value = true
  try {
    await callNext(CLINIC_ID, 'Consulting')
    await loadQueue()
  } catch (e: any) {
    alert(`叫號失敗：${e.message}`)
  } finally {
    calling.value = false
  }
}

async function handleSkip(visitId: string) {
  try {
    await skipVisit(CLINIC_ID, visitId)
    await loadQueue()
  } catch (e: any) {
    alert(`過號失敗：${e.message}`)
  }
}

async function handleManualCheckIn() {
  if (!checkinForm.value.phone && !checkinForm.value.fullName) {
    alert('請至少輸入手機號碼或姓名')
    return
  }
  checkingIn.value = true
  try {
    const result = await manualCheckIn(CLINIC_ID, {
      phone: checkinForm.value.phone || undefined,
      fullName: checkinForm.value.fullName || undefined,
    })
    alert(`報到成功！號碼：${result.queueNumber}`)
    checkinForm.value = { phone: '', fullName: '' }
    await loadQueue()
  } catch (e: any) {
    alert(`報到失敗：${e.message}`)
  } finally {
    checkingIn.value = false
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
.nurse-dashboard {
  max-width: 1200px;
  margin: 0 auto;
  padding: 16px;
}

.page-header h1 {
  font-size: 1.5rem;
  margin-bottom: 16px;
  color: #1a1a2e;
}

.dashboard-layout {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

@media (min-width: 768px) {
  .dashboard-layout {
    flex-direction: row;
  }
  .queue-panel {
    flex: 2;
  }
  .action-panel {
    flex: 1;
  }
}

.queue-panel,
.action-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.queue-panel h2,
.action-card h2 {
  font-size: 1.1rem;
  margin-bottom: 12px;
  color: #333;
}

.action-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
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
  padding: 12px;
  border-bottom: 1px solid #f0f0f0;
  gap: 8px;
}

.queue-item:last-child {
  border-bottom: none;
}

.queue-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.queue-number {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2563eb;
  min-width: 48px;
  text-align: center;
}

.patient-detail {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.patient-name {
  font-weight: 600;
  color: #1a1a2e;
}

.patient-meta {
  font-size: 0.8rem;
  color: #888;
}

.status-called {
  background: #eff6ff;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  cursor: pointer;
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

.btn-large {
  width: 100%;
  padding: 16px;
  font-size: 1.1rem;
  font-weight: 600;
}

.btn-skip {
  background: #f59e0b;
  color: #fff;
  white-space: nowrap;
}

.btn-skip:hover {
  background: #d97706;
}

.checkin-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.form-group label {
  font-size: 0.85rem;
  color: #555;
}

.form-group input {
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
}

.form-group input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.15);
}
</style>
