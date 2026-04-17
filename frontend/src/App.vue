<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed } from 'vue'

const route = useRoute()

const navItems = [
  { path: '/', label: '報到', icon: '📋' },
  { path: '/queue', label: '候診', icon: '⏳' },
  { path: '/nurse', label: '護理師', icon: '👩‍⚕️' },
  { path: '/doctor', label: '醫師', icon: '🩺' },
  { path: '/pharmacy', label: '藥局', icon: '💊' },
  { path: '/admin', label: '管理', icon: '⚙️' },
]

const currentPath = computed(() => route.path)
</script>

<template>
  <div id="layout">
    <nav class="top-nav">
      <div class="nav-brand">
        <span class="brand-icon">🏥</span>
        <span class="brand-text">門診流程管理</span>
      </div>
      <div class="nav-links">
        <router-link
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          :class="['nav-link', { active: currentPath === item.path }]"
        >
          <span class="nav-icon">{{ item.icon }}</span>
          <span class="nav-label">{{ item.label }}</span>
        </router-link>
      </div>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
#layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.top-nav {
  background: #ffffff;
  border-bottom: 1px solid #e2e8f0;
  padding: 0 16px;
  display: flex;
  align-items: center;
  height: 56px;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-right: 24px;
  flex-shrink: 0;
}

.brand-icon {
  font-size: 1.4rem;
}

.brand-text {
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
}

.nav-links {
  display: flex;
  gap: 2px;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 8px 12px;
  border-radius: 8px;
  text-decoration: none;
  color: #64748b;
  font-size: 0.85rem;
  font-weight: 500;
  white-space: nowrap;
  transition: all 0.15s;
}

.nav-link:hover {
  background: #f1f5f9;
  color: #334155;
}

.nav-link.active {
  background: #eff6ff;
  color: #2563eb;
  font-weight: 600;
}

.nav-icon {
  font-size: 1rem;
}

.main-content {
  flex: 1;
  padding-top: 8px;
}

@media (max-width: 640px) {
  .brand-text {
    display: none;
  }
  .nav-brand {
    margin-right: 8px;
  }
  .nav-link {
    padding: 8px;
  }
  .nav-label {
    display: none;
  }
  .nav-icon {
    font-size: 1.2rem;
  }
}
</style>
