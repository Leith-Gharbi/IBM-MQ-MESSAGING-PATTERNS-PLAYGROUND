<template>
  <PatternPanel
    title="Request/Reply"
    :is-loading="isLoading"
    :error="error"
    @clear="clearRequests"
  >
    <div class="reqreply-content">
      <!-- Request Section -->
      <div class="section requester">
        <h3>Send Request</h3>
        <MessageInput
          placeholder="Enter request (e.g., 'Calculate: 2+2')"
          button-text="Send Request"
          :disabled="isLoading"
          @send="sendRequest"
        />
      </div>

      <!-- Requests List -->
      <div class="section requests-list">
        <h3>Requests & Responses</h3>
        <div v-if="requests.length === 0" class="empty-state">
          No requests yet
        </div>
        <div v-else class="requests">
          <transition-group name="fade" tag="div">
            <div
              v-for="request in displayedRequests"
              :key="request.id"
              class="request-item"
              :class="request.status.toLowerCase()"
            >
              <div class="request-header">
                <span class="status-badge" :class="request.status.toLowerCase()">
                  {{ request.status }}
                </span>
                <span class="correlation-id">
                  ID: {{ request.correlationId ? request.correlationId.slice(0, 12) : 'N/A' }}...
                </span>
              </div>

              <div class="request-content">
                <div class="message sent">
                  <span class="label">Request:</span>
                  <span class="content">{{ request.content }}</span>
                  <span class="timestamp">{{ formatTime(request.sentAt) }}</span>
                </div>

                <div v-if="request.response" class="message received">
                  <span class="label">Response:</span>
                  <span class="content">{{ request.response.content }}</span>
                  <span class="timestamp">{{ formatTime(request.response.timestamp) }}</span>
                </div>

                <div v-else-if="request.status === 'Pending'" class="pending-indicator">
                  <span class="spinner"></span>
                  <span>Waiting for response...</span>
                  <span class="timeout-countdown">{{ getTimeRemaining(request) }}s remaining</span>
                </div>

                <div v-else-if="request.status === 'TimedOut'" class="timeout-message">
                  Request timed out after {{ request.timeoutSeconds }}s
                </div>
              </div>
            </div>
          </transition-group>
        </div>
      </div>
    </div>
  </PatternPanel>
</template>

<script>
import PatternPanel from '../components/PatternPanel.vue';
import MessageInput from '../components/MessageInput.vue';
import signalrService from '../services/signalrService';

/**
 * RequestReplyPanel component
 *
 * Implements the Request/Reply messaging pattern UI.
 * Users can send requests and see correlated responses.
 *
 * WHY: Request/Reply demonstrates synchronous-style communication
 * over asynchronous messaging. Each request has a correlation ID
 * that links it to its response, enabling request-response patterns.
 */
export default {
  name: 'RequestReplyPanel',

  components: {
    PatternPanel,
    MessageInput
  },

  data() {
    return {
      requests: [],
      isLoading: false,
      error: '',
      timeUpdateInterval: null
    };
  },

  computed: {
    displayedRequests() {
      // Show most recent first, limit to 50
      return [...this.requests].reverse().slice(0, 50);
    }
  },

  mounted() {
    signalrService.on('RequestStatusChanged', this.handleRequestStatusChanged);

    // Update time remaining every second
    this.timeUpdateInterval = setInterval(() => {
      this.$forceUpdate();
    }, 1000);
  },

  beforeDestroy() {
    signalrService.off('RequestStatusChanged', this.handleRequestStatusChanged);
    if (this.timeUpdateInterval) {
      clearInterval(this.timeUpdateInterval);
    }
  },

  methods: {
    handleRequestStatusChanged(updatedRequest) {
      const index = this.requests.findIndex(r => r.correlationId === updatedRequest.correlationId);
      if (index !== -1) {
        this.$set(this.requests, index, updatedRequest);
      }
    },

    async sendRequest(content) {
      this.isLoading = true;
      this.error = '';

      try {
        const response = await fetch('/api/request-reply/send', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ content, timeoutSeconds: 30 })
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to send request');
        }

        const request = await response.json();
        this.requests.push(request);

        // FIFO: Keep only last 50 requests
        if (this.requests.length > 50) {
          this.requests.shift();
        }
      } catch (err) {
        console.error('Error sending request:', err);
        this.error = err.message || 'Failed to send request';
      } finally {
        this.isLoading = false;
      }
    },

    async clearRequests() {
      try {
        await fetch('/api/request-reply/clear', { method: 'POST' });
        this.requests = [];
        this.error = '';
      } catch (err) {
        console.error('Error clearing requests:', err);
      }
    },

    formatTime(timestamp) {
      return new Date(timestamp).toLocaleTimeString();
    },

    getTimeRemaining(request) {
      if (request.status !== 'Pending') return 0;
      const elapsed = (Date.now() - new Date(request.sentAt).getTime()) / 1000;
      const remaining = Math.max(0, request.timeoutSeconds - Math.floor(elapsed));
      return remaining;
    }
  }
};
</script>

<style scoped>
.reqreply-content {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 12px;
}

.section h3 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #616161;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.requester {
  background-color: #e8f5e9;
  border-color: #a5d6a7;
}

.requests-list {
  background-color: #fafafa;
  max-height: 400px;
  overflow-y: auto;
}

.empty-state {
  color: #9e9e9e;
  font-style: italic;
  text-align: center;
  padding: 20px;
}

.requests {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.request-item {
  background-color: white;
  border-radius: 4px;
  padding: 12px;
  border-left: 4px solid #9e9e9e;
}

.request-item.pending {
  border-left-color: #ff9800;
}

.request-item.completed {
  border-left-color: #4caf50;
}

.request-item.timedout {
  border-left-color: #f44336;
}

.request-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.status-badge {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: bold;
  text-transform: uppercase;
}

.status-badge.pending {
  background-color: #fff3e0;
  color: #e65100;
}

.status-badge.completed {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.status-badge.timedout {
  background-color: #ffebee;
  color: #c62828;
}

.correlation-id {
  font-size: 11px;
  color: #757575;
  font-family: monospace;
}

.request-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.message {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: baseline;
  padding: 6px 10px;
  border-radius: 4px;
}

.message.sent {
  background-color: #e3f2fd;
}

.message.received {
  background-color: #e8f5e9;
}

.message .label {
  font-weight: 600;
  font-size: 12px;
  color: #616161;
}

.message .content {
  flex: 1;
  word-break: break-word;
}

.message .timestamp {
  font-size: 11px;
  color: #9e9e9e;
}

.pending-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #e65100;
  font-size: 13px;
  padding: 6px 10px;
  background-color: #fff3e0;
  border-radius: 4px;
}

.pending-indicator .spinner {
  width: 14px;
  height: 14px;
  border: 2px solid #ffcc80;
  border-top-color: #e65100;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.timeout-countdown {
  margin-left: auto;
  font-weight: 500;
}

.timeout-message {
  color: #c62828;
  font-size: 13px;
  padding: 6px 10px;
  background-color: #ffebee;
  border-radius: 4px;
}

/* Fade animation */
.fade-enter-active {
  transition: all 0.3s ease-out;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
