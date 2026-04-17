const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5160'
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
  // 允許空 body（如 200 OK 但無內容）
  const text = await res.text()
  return (text ? JSON.parse(text) : null) as T
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

// 醫師頁專用：撈已被叫到（Called 狀態）的病患
export const getDoctorQueue = (clinicId: string) =>
  get<Array<{ visitId: string; queueNumber: number; patientName: string; priority: number; status: string }>>(
    `/doctor/queue?clinicId=${clinicId}`)

export const callNext = (clinicId: string, queueType: string, roomId?: string) =>
  post('/queue/call-next', { clinicId, queueType, roomId })

export const skipVisit = (clinicId: string, visitId: string) =>
  post(`/visits/${visitId}/skip`, { clinicId })

// 醫師
export const startConsult = (clinicId: string, visitId: string) =>
  post(`/doctor/visits/${visitId}/start-consult?clinicId=${clinicId}`, {})

export const completeConsult = (clinicId: string, visitId: string, needsMedication: boolean) =>
  post(`/doctor/visits/${visitId}/complete-consult`, { clinicId, visitId, needsMedication })

export const createPrescription = (clinicId: string, visitId: string, items: Array<{
  medicationId: string; dosage: string; frequency: string; durationDays: number; quantity: number
}>, notes?: string) =>
  post(`/doctor/visits/${visitId}/prescriptions`, { clinicId, visitId, items, notes })

// 藥劑師
export const getPharmacyQueue = (clinicId: string) =>
  get<Array<{ prescriptionId: string; visitId: string; patientName: string; status: string }>>(
    `/pharmacy/queue?clinicId=${clinicId}`)

export const startDispense = (clinicId: string, prescriptionId: string) =>
  post(`/pharmacy/prescriptions/${prescriptionId}/start-dispense?clinicId=${clinicId}`, {})

export const completeDispense = (clinicId: string, prescriptionId: string) =>
  post(`/pharmacy/prescriptions/${prescriptionId}/complete-dispense?clinicId=${clinicId}`, {})

// AI（本地模型推理較慢，timeout 60 秒）
export const aiTriage = (clinicId: string, symptoms: string) =>
  post<{ department: string; departmentId: string | null; priority: number; estimatedWaitMinutes: number; reasoning: string }>(
    '/ai/triage', { clinicId, symptoms }, 60000)

export const aiCommand = (clinicId: string, userId: string, role: string, command: string) =>
  post<{ action: string; params: Record<string, unknown> | null; result: string; message: string }>(
    '/ai/command', { clinicId, userId, role, command }, 30000)

export const aiCommandExecute = (clinicId: string, userId: string, role: string, action: string, params: Record<string, unknown> | null) =>
  post<{ success: boolean; message: string; data: Record<string, unknown> | null }>(
    '/ai/command/execute', { clinicId, userId, role, action, params }, 10000)
