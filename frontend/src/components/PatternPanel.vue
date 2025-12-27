<template>
  <div class="pattern-panel" :class="{ loading: isLoading }">
    <div class="panel-header">
      <h2>{{ title }}</h2>
      <button
        v-if="showClearButton"
        class="clear-btn"
        @click="$emit('clear')"
        :disabled="isLoading"
      >
        Clear
      </button>
    </div>
    <div class="panel-content">
      <slot></slot>
    </div>
    <div v-if="error" class="panel-error">
      {{ error }}
    </div>
    <div v-if="isLoading" class="loading-overlay">
      <span class="spinner"></span>
    </div>
  </div>
</template>

<script>
/**
 * PatternPanel component
 *
 * Reusable wrapper for each messaging pattern panel.
 * Provides consistent styling, title, clear button, and loading state.
 *
 * @prop {String} title - Panel title
 * @prop {Boolean} isLoading - Show loading overlay
 * @prop {String} error - Error message to display
 * @prop {Boolean} showClearButton - Show clear button in header
 *
 * @emits clear - Emitted when clear button is clicked
 */
export default {
  name: 'PatternPanel',

  props: {
    title: {
      type: String,
      required: true
    },
    isLoading: {
      type: Boolean,
      default: false
    },
    error: {
      type: String,
      default: ''
    },
    showClearButton: {
      type: Boolean,
      default: true
    }
  }
};
</script>

<style scoped>
.pattern-panel {
  position: relative;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #e0e0e0;
}

.panel-header h2 {
  margin: 0;
  font-size: 18px;
  color: #333;
}

.clear-btn {
  padding: 6px 12px;
  background-color: #ff5722;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.clear-btn:hover:not(:disabled) {
  background-color: #e64a19;
}

.clear-btn:disabled {
  background-color: #bdbdbd;
  cursor: not-allowed;
}

.panel-content {
  padding: 16px;
}

.panel-error {
  padding: 12px 16px;
  background-color: #ffebee;
  color: #c62828;
  border-top: 1px solid #ef9a9a;
  font-size: 14px;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e0e0e0;
  border-top-color: #2196f3;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.loading .panel-content {
  pointer-events: none;
}
</style>
