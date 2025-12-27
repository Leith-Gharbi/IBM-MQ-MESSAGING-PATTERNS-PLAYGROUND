<template>
  <div class="queue-view">
    <div class="queue-header">
      <span class="queue-name">{{ queueName }}</span>
      <span class="queue-depth">{{ messages.length }} message(s)</span>
    </div>
    <div class="queue-container">
      <div class="queue-arrow queue-arrow-in">
        <span>IN</span>
      </div>
      <div class="queue-messages">
        <transition-group name="queue-slide" tag="div" class="messages-row">
          <div
            v-for="message in messages"
            :key="message.id"
            class="queue-message-item"
          >
            <div class="message-preview">{{ truncate(message.content, 40) }}</div>
            <div class="message-time">{{ formatTime(message.enqueuedAt) }}</div>
          </div>
        </transition-group>
        <div v-if="messages.length === 0" class="empty-queue">
          Queue is empty
        </div>
      </div>
      <div class="queue-arrow queue-arrow-out">
        <span>OUT</span>
      </div>
    </div>
  </div>
</template>

<script>
/**
 * QueueView component
 *
 * Displays the current contents of a message queue for visualization.
 * Messages slide in from the left when added and slide out to the right when consumed.
 *
 * @prop {Array} messages - Array of QueueMessage objects currently in the queue
 * @prop {String} queueName - Name of the queue being displayed
 */
export default {
  name: 'QueueView',

  props: {
    messages: {
      type: Array,
      default: () => []
    },
    queueName: {
      type: String,
      default: 'Queue'
    }
  },

  methods: {
    formatTime(timestamp) {
      const date = new Date(timestamp);
      return date.toLocaleTimeString();
    },
    truncate(text, maxLength) {
      if (!text) return '';
      if (text.length <= maxLength) return text;
      return text.slice(0, maxLength) + '...';
    }
  }
};
</script>

<style scoped>
.queue-view {
  background: linear-gradient(90deg, #e3f2fd, #bbdefb, #e3f2fd);
  border: 2px dashed #2196f3;
  border-radius: 8px;
  padding: 12px;
  margin: 12px 0;
}

.queue-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: 500;
  color: #1565c0;
}

.queue-name {
  font-family: monospace;
}

.queue-depth {
  background-color: #2196f3;
  color: white;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
}

.queue-container {
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 80px;
}

.queue-arrow {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 40px;
  flex-shrink: 0;
  color: #1976d2;
  font-size: 10px;
  font-weight: bold;
}

.queue-arrow-in::after {
  content: '';
  width: 0;
  height: 0;
  border-top: 8px solid transparent;
  border-bottom: 8px solid transparent;
  border-left: 12px solid #1976d2;
  margin-top: 4px;
}

.queue-arrow-out::after {
  content: '';
  width: 0;
  height: 0;
  border-top: 8px solid transparent;
  border-bottom: 8px solid transparent;
  border-left: 12px solid #1976d2;
  margin-top: 4px;
}

.queue-messages {
  flex: 1;
  background-color: rgba(255, 255, 255, 0.7);
  border-radius: 4px;
  padding: 8px;
  min-height: 60px;
  overflow-x: auto;
}

.messages-row {
  display: flex;
  flex-direction: row;
  gap: 8px;
  min-height: 44px;
}

.queue-message-item {
  background: white;
  border: 1px solid #64b5f6;
  border-radius: 4px;
  padding: 8px 12px;
  min-width: 120px;
  max-width: 180px;
  flex-shrink: 0;
  box-shadow: 0 2px 4px rgba(33, 150, 243, 0.2);
}

.message-preview {
  font-size: 13px;
  color: #333;
  word-break: break-word;
  line-height: 1.3;
}

.message-time {
  font-size: 11px;
  color: #757575;
  margin-top: 4px;
}

.empty-queue {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 44px;
  color: #9e9e9e;
  font-style: italic;
  font-size: 13px;
}

/* Slide-in animation for new messages (entering from left) */
.queue-slide-enter-active {
  transition: all 0.4s ease-out;
}

.queue-slide-enter-from {
  opacity: 0;
  transform: translateX(-50px);
}

.queue-slide-enter-to {
  opacity: 1;
  transform: translateX(0);
}

/* Slide-out animation for consumed messages (exiting to right) */
.queue-slide-leave-active {
  transition: all 0.4s ease-in;
}

.queue-slide-leave-from {
  opacity: 1;
  transform: translateX(0);
}

.queue-slide-leave-to {
  opacity: 0;
  transform: translateX(50px);
}

/* Move animation for remaining elements */
.queue-slide-move {
  transition: transform 0.3s ease;
}
</style>
