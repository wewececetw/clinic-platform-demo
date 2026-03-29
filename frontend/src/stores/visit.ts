import { defineStore } from 'pinia'

export const useVisitStore = defineStore('visit', {
  state: () => ({
    currentVisitId: null as string | null,
    queueNumber: null as number | null,
    currentStep: '',
    queuePosition: null as number | null,
    isConnected: false,
  }),

  actions: {
    setVisit(visitId: string, queueNumber: number, step: string) {
      this.currentVisitId = visitId
      this.queueNumber = queueNumber
      this.currentStep = step
    },

    updateStep(step: string) {
      this.currentStep = step
    },

    updatePosition(pos: number | null) {
      this.queuePosition = pos
    },

    setConnected(connected: boolean) {
      this.isConnected = connected
    },
  },
})
