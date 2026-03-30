<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useVisitStore } from '@/stores/visit'
import { sendOtp, verifyOtp, qrCheckIn, getVisitStatus, aiTriage } from '@/api/client'

const router = useRouter()
const store = useVisitStore()

const CLINIC_ID = import.meta.env.VITE_CLINIC_ID || 'demo-clinic-001'

// Tab 切換
type TabName = 'ai' | 'phone' | 'qrcode' | 'status'
const activeTab = ref<TabName>('ai')

// 手機報到
const phone = ref('')
const otpCode = ref('')
const otpSent = ref(false)
const phoneLoading = ref(false)
const phoneError = ref('')

async function handleSendOtp() {
  phoneError.value = ''
  phoneLoading.value = true
  try {
    await sendOtp(CLINIC_ID, phone.value)
    otpSent.value = true
  } catch (e: any) {
    phoneError.value = e.message || '發送驗證碼失敗'
  } finally {
    phoneLoading.value = false
  }
}

async function handlePhoneCheckIn() {
  phoneError.value = ''
  phoneLoading.value = true
  try {
    const result = await verifyOtp(CLINIC_ID, phone.value, otpCode.value)
    store.setVisit(result.visitId, result.queueNumber, result.currentStep)
    router.push('/queue')
  } catch (e: any) {
    phoneError.value = e.message || '報到失敗'
  } finally {
    phoneLoading.value = false
  }
}

// QR Code 報到
const qrToken = ref('')
const qrLoading = ref(false)
const qrError = ref('')

async function handleQrCheckIn() {
  qrError.value = ''
  qrLoading.value = true
  try {
    const result = await qrCheckIn(CLINIC_ID, qrToken.value)
    store.setVisit(result.visitId, result.queueNumber, result.currentStep)
    router.push('/queue')
  } catch (e: any) {
    qrError.value = e.message || '報到失敗'
  } finally {
    qrLoading.value = false
  }
}

// AI 智慧分流
const symptoms = ref('')
const aiLoading = ref(false)
const aiError = ref('')
const aiResult = ref<{ department: string; departmentId: string | null; priority: number; estimatedWaitMinutes: number; reasoning: string } | null>(null)

async function handleAiTriage() {
  aiError.value = ''
  aiResult.value = null
  aiLoading.value = true
  try {
    aiResult.value = await aiTriage(CLINIC_ID, symptoms.value)
  } catch (e: any) {
    aiError.value = e.message || 'AI 分流失敗'
  } finally {
    aiLoading.value = false
  }
}

// 查詢進度
const queryVisitId = ref('')
const queryLoading = ref(false)
const queryError = ref('')
const queryResult = ref<{ visitId: string; currentStep: string; stepName: string; status: string } | null>(null)

async function handleQueryStatus() {
  queryError.value = ''
  queryResult.value = null
  queryLoading.value = true
  try {
    queryResult.value = await getVisitStatus(queryVisitId.value)
  } catch (e: any) {
    queryError.value = e.message || '查詢失敗'
  } finally {
    queryLoading.value = false
  }
}
</script>

<template>
  <div class="checkin-page">
    <h1 class="title">門診報到</h1>

    <!-- Tab 切換 -->
    <div class="tabs">
      <button
        :class="['tab-btn', { active: activeTab === 'ai' }]"
        @click="activeTab = 'ai'"
      >
        AI 分流
      </button>
      <button
        :class="['tab-btn', { active: activeTab === 'phone' }]"
        @click="activeTab = 'phone'"
      >
        手機報到
      </button>
      <button
        :class="['tab-btn', { active: activeTab === 'qrcode' }]"
        @click="activeTab = 'qrcode'"
      >
        QR Code
      </button>
      <button
        :class="['tab-btn', { active: activeTab === 'status' }]"
        @click="activeTab = 'status'"
      >
        查詢進度
      </button>
    </div>

    <!-- AI 智慧分流 -->
    <div v-if="activeTab === 'ai'" class="tab-content">
      <div class="form-group">
        <label for="symptoms">請描述您的症狀</label>
        <textarea
          id="symptoms"
          v-model="symptoms"
          rows="3"
          placeholder="例如：頭痛三天、有點發燒、喉嚨痛..."
          class="symptoms-input"
        ></textarea>
        <button
          class="btn primary"
          :disabled="!symptoms.trim() || aiLoading"
          @click="handleAiTriage"
        >
          {{ aiLoading ? 'AI 分析中...' : 'AI 智慧分流' }}
        </button>
      </div>

      <div v-if="aiLoading" class="ai-loading">
        <div class="spinner"></div>
        <p>AI 正在分析您的症狀...</p>
      </div>

      <div v-if="aiResult" class="ai-result">
        <div class="ai-result-header">AI 建議</div>
        <div class="ai-result-body">
          <div class="ai-field">
            <span class="ai-label">建議科別</span>
            <span class="ai-value department">{{ aiResult.department }}</span>
          </div>
          <div class="ai-field">
            <span class="ai-label">優先度</span>
            <span class="ai-value" :class="'priority-' + aiResult.priority">
              {{ ['一般', '優先', '緊急'][aiResult.priority] || '一般' }}
            </span>
          </div>
          <div class="ai-field">
            <span class="ai-label">預估等候</span>
            <span class="ai-value">約 {{ aiResult.estimatedWaitMinutes }} 分鐘</span>
          </div>
          <div class="ai-reasoning">
            <span class="ai-label">分析說明</span>
            <p>{{ aiResult.reasoning }}</p>
          </div>
        </div>
        <p class="ai-disclaimer">此為 AI 建議，僅供參考。實際科別由醫護人員確認。</p>
      </div>

      <p v-if="aiError" class="error">{{ aiError }}</p>
    </div>

    <!-- 手機報到 -->
    <div v-if="activeTab === 'phone'" class="tab-content">
      <div class="form-group">
        <label for="phone">手機號碼</label>
        <input
          id="phone"
          v-model="phone"
          type="tel"
          placeholder="0912345678"
          :disabled="otpSent"
        />
        <button
          class="btn primary"
          :disabled="!phone || otpSent || phoneLoading"
          @click="handleSendOtp"
        >
          {{ phoneLoading && !otpSent ? '發送中...' : '發送驗證碼' }}
        </button>
      </div>

      <div v-if="otpSent" class="form-group">
        <label for="otpCode">驗證碼</label>
        <input
          id="otpCode"
          v-model="otpCode"
          type="text"
          inputmode="numeric"
          placeholder="請輸入驗證碼"
        />
        <button
          class="btn primary"
          :disabled="!otpCode || phoneLoading"
          @click="handlePhoneCheckIn"
        >
          {{ phoneLoading ? '報到中...' : '報到' }}
        </button>
      </div>

      <p v-if="phoneError" class="error">{{ phoneError }}</p>
    </div>

    <!-- QR Code 報到 -->
    <div v-if="activeTab === 'qrcode'" class="tab-content">
      <div class="form-group">
        <label for="qrToken">QR Code Token</label>
        <input
          id="qrToken"
          v-model="qrToken"
          type="text"
          placeholder="請輸入或掃描 QR Code"
        />
        <button
          class="btn primary"
          :disabled="!qrToken || qrLoading"
          @click="handleQrCheckIn"
        >
          {{ qrLoading ? '報到中...' : '報到' }}
        </button>
      </div>

      <p v-if="qrError" class="error">{{ qrError }}</p>
    </div>

    <!-- 查詢進度 -->
    <div v-if="activeTab === 'status'" class="tab-content">
      <div class="form-group">
        <label for="queryVisitId">門診編號 (Visit ID)</label>
        <input
          id="queryVisitId"
          v-model="queryVisitId"
          type="text"
          placeholder="請輸入門診編號"
        />
        <button
          class="btn primary"
          :disabled="!queryVisitId || queryLoading"
          @click="handleQueryStatus"
        >
          {{ queryLoading ? '查詢中...' : '查詢' }}
        </button>
      </div>

      <div v-if="queryResult" class="status-card">
        <p><strong>門診編號：</strong>{{ queryResult.visitId }}</p>
        <p><strong>目前步驟：</strong>{{ queryResult.stepName }}</p>
        <p><strong>狀態：</strong>{{ queryResult.status }}</p>
      </div>

      <p v-if="queryError" class="error">{{ queryError }}</p>
    </div>
  </div>
</template>

<style scoped>
.checkin-page {
  max-width: 420px;
  margin: 0 auto;
  padding: 24px 16px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.title {
  text-align: center;
  font-size: 24px;
  margin-bottom: 24px;
  color: #1a1a1a;
}

.tabs {
  display: flex;
  gap: 4px;
  margin-bottom: 20px;
  background: #f0f0f0;
  border-radius: 8px;
  padding: 4px;
}

.tab-btn {
  flex: 1;
  padding: 10px 8px;
  border: none;
  background: transparent;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  color: #666;
  transition: all 0.2s;
}

.tab-btn.active {
  background: #fff;
  color: #1a73e8;
  font-weight: 600;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-content {
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #333;
  margin-bottom: 6px;
}

.form-group input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 16px;
  box-sizing: border-box;
  margin-bottom: 10px;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #1a73e8;
  box-shadow: 0 0 0 2px rgba(26, 115, 232, 0.15);
}

.form-group input:disabled {
  background: #f5f5f5;
  color: #999;
}

.btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn.primary {
  background: #1a73e8;
  color: #fff;
}

.btn.primary:hover:not(:disabled) {
  background: #1557b0;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error {
  color: #d93025;
  font-size: 14px;
  margin-top: 8px;
  padding: 8px 12px;
  background: #fce8e6;
  border-radius: 6px;
}

.symptoms-input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 16px;
  font-family: inherit;
  box-sizing: border-box;
  margin-bottom: 10px;
  resize: vertical;
  min-height: 80px;
  transition: border-color 0.2s;
}

.symptoms-input:focus {
  outline: none;
  border-color: #1a73e8;
  box-shadow: 0 0 0 2px rgba(26, 115, 232, 0.15);
}

.ai-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 24px 0;
  gap: 12px;
  color: #666;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e0e0e0;
  border-top-color: #1a73e8;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.ai-result {
  margin-top: 16px;
  border: 1px solid #c8e6c9;
  border-radius: 12px;
  overflow: hidden;
}

.ai-result-header {
  background: #e8f5e9;
  color: #2e7d32;
  font-weight: 600;
  padding: 10px 16px;
  font-size: 14px;
}

.ai-result-body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ai-field {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.ai-label {
  font-size: 13px;
  color: #888;
}

.ai-value {
  font-size: 15px;
  font-weight: 600;
  color: #333;
}

.ai-value.department {
  color: #1a73e8;
  font-size: 18px;
}

.priority-0 { color: #333; }
.priority-1 { color: #f59e0b; }
.priority-2 { color: #dc2626; font-weight: 700; }

.ai-reasoning {
  border-top: 1px solid #f0f0f0;
  padding-top: 12px;
}

.ai-reasoning p {
  font-size: 14px;
  color: #555;
  line-height: 1.5;
  margin-top: 4px;
}

.ai-disclaimer {
  font-size: 12px;
  color: #999;
  padding: 8px 16px 12px;
  text-align: center;
}

.status-card {
  background: #f8f9fa;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 16px;
  margin-top: 12px;
}

.status-card p {
  margin: 6px 0;
  font-size: 15px;
  color: #333;
}
</style>
