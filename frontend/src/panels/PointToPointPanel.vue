<template>
  <PatternPanel
    title="Point-to-Point"
    :is-loading="isLoading"
    :error="error"
    @clear="clearMessages"
  >
    <div class="p2p-content">
      <!-- Producer Section -->
      <div class="section producer">
        <h3>Producer</h3>
        <MessageInput
          placeholder="Type a message to send..."
          button-text="Send"
          :disabled="isLoading"
          @send="sendMessage"
        />
      </div>

      <!-- Consumer Section -->
      <div class="section consumer">
        <h3>Consumer</h3>
        <MessageList
          :messages="messages"
          empty-text="No messages received yet"
          :max-messages="50"
        />
      </div>
    </div>
  </PatternPanel>
</template>

<script>
import PatternPanel from '../components/PatternPanel.vue';
import MessageInput from '../components/MessageInput.vue';
import MessageList from '../components/MessageList.vue';
import signalrService from '../services/signalrService';

/**
 * PointToPointPanel component
 *
 * Implements the Point-to-Point messaging pattern UI.
 * Users can send messages to a queue and see them consumed in real-time.
 *
 * WHY: Point-to-Point demonstrates the most basic MQ pattern where
 * messages flow from one producer to one consumer through a queue.
 * Each message is consumed exactly once.
 */
export default {
  name: 'PointToPointPanel',

  components: {
    PatternPanel,
    MessageInput,
    MessageList
  },

  data() {
    return {
      messages: [],
      isLoading: false,
      error: ''
    };
  },

  mounted() {
    // Register for Point-to-Point messages
    signalrService.on('MessageReceived', this.handleMessageReceived);
  },

  beforeDestroy() {
    signalrService.off('MessageReceived', this.handleMessageReceived);
  },

  methods: {
    handleMessageReceived(message) {
      // Only handle Point-to-Point messages
      if (message.pattern === 'PointToPoint') {
        this.messages.push(message);

        // FIFO: Keep only last 50 messages
        if (this.messages.length > 50) {
          this.messages.shift();
        }
      }
    },

    async sendMessage(content) {
      this.isLoading = true;
      this.error = '';

      try {
        const response = await fetch('/api/point-to-point/send', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ content })
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to send message');
        }

        // Message sent successfully
        // The consumer will receive it and broadcast via SignalR
      } catch (err) {
        console.error('Error sending message:', err);
        this.error = err.message || 'Failed to send message';
      } finally {
        this.isLoading = false;
      }
    },

    async clearMessages() {
      try {
        await fetch('/api/point-to-point/clear', {
          method: 'POST'
        });
        this.messages = [];
        this.error = '';
      } catch (err) {
        console.error('Error clearing messages:', err);
      }
    }
  }
};
</script>

<style scoped>
.p2p-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
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

.producer {
  background-color: #e8f5e9;
  border-color: #a5d6a7;
}

.consumer {
  background-color: #fff3e0;
  border-color: #ffcc80;
}
</style>
