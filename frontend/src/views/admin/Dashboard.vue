<template>
  <div class="admin-dashboard">
    <header class="page-header">
      <h1>管理員後台</h1>
      <p class="subtitle">開發中</p>
    </header>

    <!-- AI 指令 -->
    <section class="card">
      <h2>AI 指令</h2>
      <CommandInput
        :clinic-id="CLINIC_ID"
        :user-id="USER_ID"
        role="Admin"
        placeholder="輸入指令：今天看了幾個病人、還有幾位候診..."
      />
    </section>

    <section class="card">
      <h2>功能清單</h2>
      <ul class="feature-list">
        <li v-for="feature in features" :key="feature.name" class="feature-item">
          <span class="feature-icon">{{ feature.icon }}</span>
          <div class="feature-info">
            <span class="feature-name">{{ feature.name }}</span>
            <span class="feature-desc">{{ feature.description }}</span>
          </div>
          <span class="badge">即將推出</span>
        </li>
      </ul>
    </section>
  </div>
</template>

<script setup lang="ts">
import CommandInput from '@/components/CommandInput.vue'

const CLINIC_ID = '10000000-0000-0000-0000-000000000001'
const USER_ID = '20000000-0000-0000-0000-000000000003'

const features = [
  { name: '流程管理', description: '自訂門診流程步驟與轉場規則', icon: '\u{1F504}' },
  { name: '叫號規則', description: '設定叫號邏輯與優先度權重', icon: '\u{1F4E2}' },
  { name: '科別管理', description: '新增、編輯、停用門診科別', icon: '\u{1F3E5}' },
  { name: '診間管理', description: '管理診間配置與看診排程', icon: '\u{1FA7A}' },
  { name: '藥品管理', description: '維護藥品資料庫與庫存', icon: '\u{1F48A}' },
  { name: '排班管理', description: '醫師與護理師排班表', icon: '\u{1F4C5}' },
]
</script>

<style scoped>
.admin-dashboard {
  max-width: 800px;
  margin: 0 auto;
  padding: 16px;
}

.page-header {
  margin-bottom: 16px;
}

.page-header h1 {
  font-size: 1.5rem;
  color: #1a1a2e;
  margin-bottom: 4px;
}

.subtitle {
  color: #999;
  font-size: 0.9rem;
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

.feature-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 8px;
  border-bottom: 1px solid #f0f0f0;
}

.feature-item:last-child {
  border-bottom: none;
}

.feature-icon {
  font-size: 1.6rem;
  min-width: 40px;
  text-align: center;
}

.feature-info {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.feature-name {
  font-weight: 600;
  color: #1a1a2e;
}

.feature-desc {
  font-size: 0.8rem;
  color: #888;
  margin-top: 2px;
}

.badge {
  font-size: 0.75rem;
  padding: 2px 10px;
  border-radius: 12px;
  background: #f3f4f6;
  color: #6b7280;
  white-space: nowrap;
}
</style>
