<template>
  <div class="doctor-consult">
    <header class="page-header">
      <h1>醫師看診</h1>
    </header>

    <!-- AI 指令 -->
    <section class="card">
      <h2>AI 指令</h2>
      <CommandInput
        :clinic-id="CLINIC_ID"
        :user-id="USER_ID"
        role="Doctor"
        placeholder="輸入指令：完成看診、需要拿藥、還有幾位候診..."
        @executed="loadCurrentPatient"
      />
    </section>

    <!-- 目前叫到的病患 -->
    <section class="current-patient card">
      <h2>目前病患</h2>
      <div v-if="loading" class="loading-text">載入中...</div>
      <div v-else-if="!currentPatient" class="empty-text">目前無叫到的病患</div>
      <div v-else class="patient-info">
        <span class="queue-number">{{ currentPatient.queueNumber }}</span>
        <div class="patient-detail">
          <span class="patient-name">{{ currentPatient.patientName }}</span>
          <span class="patient-status">狀態：{{ currentPatient.status }}</span>
        </div>
        <button
          v-if="!consulting"
          class="btn btn-primary"
          :disabled="startingConsult"
          @click="handleStartConsult"
        >
          {{ startingConsult ? '處理中...' : '開始看診' }}
        </button>
      </div>
    </section>

    <!-- 看診區 -->
    <section v-if="consulting && currentPatient" class="consult-area card">
      <h2>看診中 — {{ currentPatient.patientName }}</h2>

      <div class="form-group checkbox-group">
        <label>
          <input type="checkbox" v-model="needsMedication" />
          需要領藥
        </label>
      </div>

      <button
        class="btn btn-success"
        :disabled="completing"
        @click="handleCompleteConsult"
      >
        {{ completing ? '處理中...' : '完成看診' }}
      </button>
    </section>

    <!-- 處方區 -->
    <section v-if="showPrescription && currentPatient" class="prescription-area card">
      <h2>開立處方</h2>
      <form @submit.prevent="handleCreatePrescription" class="prescription-form">
        <div class="form-row">
          <div class="form-group">
            <label>藥品名稱</label>
            <input v-model="rxForm.medicationName" type="text" placeholder="藥品名稱" required />
          </div>
          <div class="form-group">
            <label>劑量</label>
            <input v-model="rxForm.dosage" type="text" placeholder="例：500mg" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>頻次</label>
            <input v-model="rxForm.frequency" type="text" placeholder="例：TID" required />
          </div>
          <div class="form-group">
            <label>天數</label>
            <input v-model.number="rxForm.durationDays" type="number" min="1" placeholder="天數" required />
          </div>
        </div>
        <div class="form-group">
          <label>數量</label>
          <input v-model.number="rxForm.quantity" type="number" min="1" placeholder="總量" required />
        </div>
        <button class="btn btn-primary" type="submit" :disabled="prescribing">
          {{ prescribing ? '處理中...' : '開立處方' }}
        </button>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { getDoctorQueue, startConsult, completeConsult, createPrescription } from '@/api/client'
import CommandInput from '@/components/CommandInput.vue'

const CLINIC_ID = '10000000-0000-0000-0000-000000000001'
const USER_ID = '20000000-0000-0000-0000-000000000002'

interface QueueItem {
  visitId: string
  queueNumber: number
  patientName: string
  priority: number
  status: string
}

const loading = ref(true)
const currentPatient = ref<QueueItem | null>(null)
const consulting = ref(false)
const startingConsult = ref(false)
const completing = ref(false)
const prescribing = ref(false)
const needsMedication = ref(true)
const showPrescription = ref(false)

const rxForm = ref({
  medicationName: '',
  dosage: '',
  frequency: '',
  durationDays: 3,
  quantity: 9,
})

let timer: ReturnType<typeof setInterval> | null = null

async function loadCurrentPatient() {
  try {
    const queue = await getDoctorQueue(CLINIC_ID)
    currentPatient.value = queue[0] || null
  } catch (e: any) {
    console.error('載入病患失敗', e)
  } finally {
    loading.value = false
  }
}

async function handleStartConsult() {
  if (!currentPatient.value) return
  startingConsult.value = true
  try {
    await startConsult(CLINIC_ID, currentPatient.value.visitId)
    consulting.value = true
  } catch (e: any) {
    alert(`開始看診失敗：${e.message}`)
  } finally {
    startingConsult.value = false
  }
}

async function handleCompleteConsult() {
  if (!currentPatient.value) return
  completing.value = true
  try {
    await completeConsult(CLINIC_ID, currentPatient.value.visitId, needsMedication.value)
    consulting.value = false
    if (needsMedication.value) {
      showPrescription.value = true
    } else {
      resetState()
      await loadCurrentPatient()
    }
  } catch (e: any) {
    alert(`完成看診失敗：${e.message}`)
  } finally {
    completing.value = false
  }
}

async function handleCreatePrescription() {
  if (!currentPatient.value) return
  prescribing.value = true
  try {
    await createPrescription(CLINIC_ID, currentPatient.value.visitId, [
      {
        medicationId: rxForm.value.medicationName,
        dosage: rxForm.value.dosage,
        frequency: rxForm.value.frequency,
        durationDays: rxForm.value.durationDays,
        quantity: rxForm.value.quantity,
      },
    ])
    alert('處方開立成功')
    resetState()
    await loadCurrentPatient()
  } catch (e: any) {
    alert(`開立處方失敗：${e.message}`)
  } finally {
    prescribing.value = false
  }
}

function resetState() {
  consulting.value = false
  showPrescription.value = false
  needsMedication.value = true
  rxForm.value = { medicationName: '', dosage: '', frequency: '', durationDays: 3, quantity: 9 }
}

onMounted(() => {
  loadCurrentPatient()
  timer = setInterval(() => {
    if (!consulting.value && !showPrescription.value) {
      loadCurrentPatient()
    }
  }, 5000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<style scoped>
.doctor-consult {
  max-width: 800px;
  margin: 0 auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header h1 {
  font-size: 1.5rem;
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

.patient-info {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.queue-number {
  font-size: 2rem;
  font-weight: 700;
  color: #2563eb;
  min-width: 56px;
  text-align: center;
}

.patient-detail {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.patient-name {
  font-size: 1.2rem;
  font-weight: 600;
  color: #1a1a2e;
}

.patient-status {
  font-size: 0.85rem;
  color: #888;
}

.checkbox-group label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 1rem;
  cursor: pointer;
}

.checkbox-group input[type='checkbox'] {
  width: 20px;
  height: 20px;
  accent-color: #2563eb;
}

.prescription-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-row {
  display: flex;
  gap: 12px;
}

@media (max-width: 480px) {
  .form-row {
    flex-direction: column;
  }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
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

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
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

.btn-success {
  background: #16a34a;
  color: #fff;
}

.btn-success:hover:not(:disabled) {
  background: #15803d;
}
</style>
