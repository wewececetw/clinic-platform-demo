<template>
  <div class="command-input-wrapper">
    <form @submit.prevent="handleSubmit" class="command-form">
      <input
        v-model="commandText"
        type="text"
        :placeholder="placeholder"
        :disabled="processing"
        class="command-input"
      />
      <button class="btn btn-command" type="submit" :disabled="processing || !commandText.trim()">
        {{ processing ? '解析中...' : '送出' }}
      </button>
    </form>

    <!-- 結果訊息 -->
    <div v-if="resultMessage" class="command-result" :class="resultType">
      <p>{{ resultMessage }}</p>
      <div v-if="showConfirm" class="confirm-actions">
        <button class="btn btn-confirm" :disabled="executing" @click="handleConfirm">
          {{ executing ? '執行中...' : '確認執行' }}
        </button>
        <button class="btn btn-cancel" :disabled="executing" @click="handleCancel">取消</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { aiCommand, aiCommandExecute } from '@/api/client'

const props = defineProps<{
  clinicId: string
  userId: string
  role: string
  placeholder?: string
}>()

const emit = defineEmits<{
  executed: []
}>()

const commandText = ref('')
const processing = ref(false)
const executing = ref(false)
const resultMessage = ref('')
const resultType = ref<'info' | 'success' | 'error'>('info')
const showConfirm = ref(false)

const pendingAction = ref('')
const pendingParams = ref<Record<string, unknown> | null>(null)

const placeholder = computed(() => props.placeholder ?? '輸入指令，例如：叫下一位、還有幾位候診...')

async function handleSubmit() {
  const text = commandText.value.trim()
  if (!text) return

  processing.value = true
  resultMessage.value = ''
  showConfirm.value = false

  try {
    const res = await aiCommand(props.clinicId, props.userId, props.role, text)
    resultMessage.value = res.message
    resultType.value = 'info'

    if (res.result === 'confirm') {
      showConfirm.value = true
      pendingAction.value = res.action
      pendingParams.value = res.params
    } else {
      // 查詢類直接顯示結果
      resultType.value = 'success'
      commandText.value = ''
    }
  } catch (e: any) {
    resultMessage.value = e.message || '指令解析失敗'
    resultType.value = 'error'
  } finally {
    processing.value = false
  }
}

async function handleConfirm() {
  executing.value = true
  try {
    const res = await aiCommandExecute(
      props.clinicId, props.userId, props.role,
      pendingAction.value, pendingParams.value
    )
    resultMessage.value = res.message
    resultType.value = res.success ? 'success' : 'error'
    showConfirm.value = false
    commandText.value = ''
    if (res.success) emit('executed')
  } catch (e: any) {
    resultMessage.value = e.message || '執行失敗'
    resultType.value = 'error'
  } finally {
    executing.value = false
  }
}

function handleCancel() {
  showConfirm.value = false
  resultMessage.value = ''
  pendingAction.value = ''
  pendingParams.value = null
}
</script>

<style scoped>
.command-input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.command-form {
  display: flex;
  gap: 8px;
}

.command-input {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
}

.command-input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.15);
}

.command-input:disabled {
  background: #f5f5f5;
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

.btn-command {
  background: #8b5cf6;
  color: #fff;
  white-space: nowrap;
}

.btn-command:hover:not(:disabled) {
  background: #7c3aed;
}

.command-result {
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 0.9rem;
}

.command-result.info {
  background: #eff6ff;
  color: #1e40af;
}

.command-result.success {
  background: #f0fdf4;
  color: #166534;
}

.command-result.error {
  background: #fef2f2;
  color: #991b1b;
}

.command-result p {
  margin: 0;
}

.confirm-actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}

.btn-confirm {
  background: #22c55e;
  color: #fff;
}

.btn-confirm:hover:not(:disabled) {
  background: #16a34a;
}

.btn-cancel {
  background: #e5e7eb;
  color: #374151;
}

.btn-cancel:hover:not(:disabled) {
  background: #d1d5db;
}
</style>
