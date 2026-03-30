const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5160'
const API = `${BASE_URL}/api`

async function request<T>(endpoint: string, options: RequestInit = {}, timeoutMs = 3000): Promise<T> {
  const controller = new AbortController()
  const timeout = setTimeout(() => controller.abort(), timeoutMs)
  const res = await fetch(`${API}${endpoint}`, {
    headers: { 'Content-Type': 'application/json' },
    signal: controller.signal,
    ...options,
  })
  clearTimeout(timeout)
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.error || res.statusText)
  }
  return res.json()
}

function post<T>(endpoint: string, body: unknown, timeoutMs = 3000): Promise<T> {
  return request<T>(endpoint, { method: 'POST', body: JSON.stringify(body) }, timeoutMs)
}

function get<T>(endpoint: string): Promise<T> {
  return request<T>(endpoint, { method: 'GET' })
}

// 報到
export const sendOtp = (clinicId: string, phone: string) =>
  post('/checkin/otp/send', { clinicId, phone })

export const verifyOtp = (clinicId: string, phone: string, otpCode: string) =>
  post<{ visitId: string; queueNumber: number; currentStep: string }>(
    '/checkin/otp/verify', { clinicId, phone, otpCode })

export const qrCheckIn = (clinicId: string, qrCodeToken: string) =>
  post<{ visitId: string; queueNumber: number; currentStep: string }>(
    '/checkin/qrcode', { clinicId, qrCodeToken })

export const manualCheckIn = (clinicId: string, data: {
  phone?: string; fullName?: string; departmentId?: string; doctorId?: string
}) =>
  post<{ visitId: string; queueNumber: number; currentStep: string }>(
    '/checkin/manual', { clinicId, ...data })

// 門診狀態
export const getVisitStatus = (visitId: string) =>
  get<{ visitId: string; currentStep: string; stepName: string; status: string }>(
    `/visits/${visitId}/status`)

// 佇列
export const getQueuePosition = (clinicId: string, visitId: string) =>
  get<{ position: number; total: number }>(
    `/queue/${clinicId}/position?visitId=${visitId}`)

export const getQueue = (clinicId: string, queueType: string) =>
  get<Array<{ visitId: string; queueNumber: number; patientName: string; priority: number; status: string }>>(
    `/queue/${clinicId}?type=${queueType}`)

export const callNext = (clinicId: string, queueType: string, roomId?: string) =>
  post('/queue/call-next', { clinicId, queueType, roomId })

export const skipVisit = (clinicId: string, visitId: string) =>
  post(`/visits/${visitId}/skip`, { clinicId })

// 醫師
export const startConsult = (visitId: string) =>
  post(`/visits/${visitId}/start-consult`, {})

export const completeConsult = (visitId: string, needsMedication: boolean) =>
  post(`/visits/${visitId}/complete-consult`, { needsMedication })

export const createPrescription = (visitId: string, items: Array<{
  medicationId: string; dosage: string; frequency: string; durationDays: number; quantity: number
}>, notes?: string) =>
  post(`/visits/${visitId}/prescriptions`, { items, notes })

// 藥劑師
export const getPharmacyQueue = () =>
  get<Array<{ prescriptionId: string; visitId: string; patientName: string; status: string }>>('/pharmacy/queue')

export const startDispense = (prescriptionId: string) =>
  post(`/prescriptions/${prescriptionId}/start-dispense`, {})

export const completeDispense = (prescriptionId: string) =>
  post(`/prescriptions/${prescriptionId}/complete-dispense`, {})

// AI（本地模型推理較慢，timeout 60 秒）
export const aiTriage = (clinicId: string, symptoms: string) =>
  post<{ department: string; departmentId: string | null; priority: number; estimatedWaitMinutes: number; reasoning: string }>(
    '/ai/triage', { clinicId, symptoms }, 60000)
