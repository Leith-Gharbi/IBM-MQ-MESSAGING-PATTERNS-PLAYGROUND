<template>
  <div class="message-list">
    <div v-if="messages.length === 0" class="empty-state">
      {{ emptyText }}
    </div>
    <transition-group name="fade" tag="div" class="messages">
      <div
        v-for="message in displayedMessages"
        :key="message.id"
        class="message-item"
        :class="message.direction.toLowerCase()"
      >
        <div class="message-content">{{ message.content }}</div>
        <div class="message-meta">
          <span class="timestamp">{{ formatTime(message.timestamp) }}</span>
          <span v-if="message.correlationId" class="correlation-id">
            ID: {{ message.correlationId.slice(0, 8) }}...
          </span>
        </div>
      </div>
    </transition-group>
  </div>
</template>

<script>
/**
 * MessageList component
 *
 * Displays a list of messages with timestamp, direction, and optional correlation ID.
 * New messages appear with a CSS fade-in animation (FR-002).
 *
 * @prop {Array} messages - Array of Message objects to display
 * @prop {String} emptyText - Text to show when no messages
 * @prop {Number} maxMessages - Maximum messages to display (default 50)
 */
export default {
  name: 'MessageList',

  props: {
    messages: {
      type: Array,
      default: () => []
    },
    emptyText: {
      type: String,
      default: 'No messages yet'
    },
    maxMessages: {
      type: Number,
      default: 50
    }
  },

  computed: {
    displayedMessages() {
      // FIFO: show most recent messages, limit to maxMessages
      return this.messages.slice(-this.maxMessages);
    }
  },

  methods: {
    formatTime(timestamp) {
      const date = new Date(timestamp);
      return date.toLocaleTimeString();
    }
  }
};
</script>

<style scoped>
.message-list {
  height: 300px;
  overflow-y: auto;
  padding: 8px;
  background-color: #fafafa;
  border-radius: 4px;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #9e9e9e;
  font-style: italic;
}

.messages {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.message-item {
  padding: 10px;
  border-radius: 4px;
  background-color: white;
  border-left: 3px solid #2196f3;
}

.message-item.sent {
  border-left-color: #4caf50;
}

.message-item.received {
  border-left-color: #ff9800;
}

.message-content {
  font-size: 14px;
  word-break: break-word;
}

.message-meta {
  display: flex;
  gap: 12px;
  margin-top: 6px;
  font-size: 12px;
  color: #757575;
}

.correlation-id {
  font-family: monospace;
}

/* Fade-in animation for new messages (FR-002) */
.fade-enter-active {
  transition: all 0.3s ease-out;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.fade-enter-to {
  opacity: 1;
  transform: translateY(0);
}
</style>
