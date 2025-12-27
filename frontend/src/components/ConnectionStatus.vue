<template>
  <div class="connection-status" :class="statusClass">
    <span class="indicator"></span>
    <span class="text">{{ statusText }}</span>
  </div>
</template>

<script>
/**
 * ConnectionStatus component
 *
 * Displays the current MQ connection state with a visual indicator.
 *
 * @prop {Boolean} isConnected - Whether connected to MQ
 * @prop {String} queueManagerName - Name of the connected Queue Manager
 * @prop {String} errorMessage - Error message if disconnected
 */
export default {
  name: 'ConnectionStatus',

  props: {
    isConnected: {
      type: Boolean,
      default: false
    },
    queueManagerName: {
      type: String,
      default: ''
    },
    errorMessage: {
      type: String,
      default: ''
    }
  },

  computed: {
    statusClass() {
      return this.isConnected ? 'connected' : 'disconnected';
    },

    statusText() {
      if (this.isConnected) {
        return `Connected to ${this.queueManagerName || 'MQ'}`;
      }
      return this.errorMessage || 'Disconnected';
    }
  }
};
</script>

<style scoped>
.connection-status {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 4px;
  font-size: 14px;
}

.indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.connected {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.connected .indicator {
  background-color: #4caf50;
}

.disconnected {
  background-color: #ffebee;
  color: #c62828;
}

.disconnected .indicator {
  background-color: #f44336;
}
</style>
