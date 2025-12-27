<template>
  <PatternPanel
    title="Publish/Subscribe"
    :is-loading="isLoading"
    :error="error"
    @clear="clearMessages"
  >
    <div class="pubsub-content">
      <!-- Publisher Section -->
      <div class="section publisher">
        <h3>Publisher</h3>
        <MessageInput
          placeholder="Type a message to publish..."
          button-text="Publish"
          :disabled="isLoading"
          @send="publishMessage"
        />
      </div>

      <!-- Topic Queue Visualization -->
      <QueueView
        :messages="queueMessages"
        queue-name="playground/pubsub/"
      />

      <!-- Subscriber Controls -->
      <SubscriberControl
        :subscriber-count="subscribers.length"
        :min-subscribers="2"
        :max-subscribers="4"
        :disabled="isLoading"
        @add="addSubscriber"
        @remove="removeLastSubscriber"
      />

      <!-- Subscriber Sections -->
      <div class="subscribers">
        <div
          v-for="subscriber in subscribers"
          :key="subscriber.id"
          class="section subscriber"
          :class="{ inactive: !subscriber.isActive }"
        >
          <div class="subscriber-header">
            <h3>{{ subscriber.name }}</h3>
            <span class="message-count">{{ subscriber.messageCount }} received</span>
          </div>
          <MessageList
            :messages="subscriberMessages[subscriber.id] || []"
            :empty-text="`No messages for ${subscriber.name}`"
            :max-messages="50"
          />
        </div>
      </div>
    </div>
  </PatternPanel>
</template>

<script>
import PatternPanel from '../components/PatternPanel.vue';
import MessageInput from '../components/MessageInput.vue';
import MessageList from '../components/MessageList.vue';
import SubscriberControl from '../components/SubscriberControl.vue';
import QueueView from '../components/QueueView.vue';
import signalrService from '../services/signalrService';

/**
 * PubSubPanel component
 *
 * Implements the Publish/Subscribe messaging pattern UI.
 * Users can publish messages and see them delivered to multiple subscribers.
 *
 * WHY: Pub/Sub demonstrates one-to-many message distribution. When a
 * message is published to a topic, all active subscribers receive
 * their own copy of the message simultaneously.
 */
export default {
  name: 'PubSubPanel',

  components: {
    PatternPanel,
    MessageInput,
    MessageList,
    SubscriberControl,
    QueueView
  },

  data() {
    return {
      subscribers: [],
      subscriberMessages: {},
      queueMessages: [],
      isLoading: false,
      error: ''
    };
  },

  async mounted() {
    // Register SignalR event handlers
    signalrService.on('SubscriberMessageReceived', this.handleSubscriberMessageReceived);
    signalrService.on('SubscriberAdded', this.handleSubscriberAdded);
    signalrService.on('SubscriberRemoved', this.handleSubscriberRemoved);
    // Register for queue visualization events
    signalrService.on('QueueMessageAdded', this.handleQueueMessageAdded);
    signalrService.on('QueueMessageRemoved', this.handleQueueMessageRemoved);

    // Load initial subscribers
    await this.loadSubscribers();
  },

  beforeDestroy() {
    signalrService.off('SubscriberMessageReceived', this.handleSubscriberMessageReceived);
    signalrService.off('SubscriberAdded', this.handleSubscriberAdded);
    signalrService.off('SubscriberRemoved', this.handleSubscriberRemoved);
    signalrService.off('QueueMessageAdded', this.handleQueueMessageAdded);
    signalrService.off('QueueMessageRemoved', this.handleQueueMessageRemoved);
  },

  methods: {
    async loadSubscribers() {
      try {
        const response = await fetch('/api/pubsub/subscribers');
        if (response.ok) {
          this.subscribers = await response.json();
          // Initialize message arrays for each subscriber
          this.subscribers.forEach(sub => {
            if (!this.subscriberMessages[sub.id]) {
              this.$set(this.subscriberMessages, sub.id, []);
            }
          });
        }
      } catch (err) {
        console.error('Failed to load subscribers:', err);
      }
    },

    handleSubscriberMessageReceived(subscriberId, message) {
      if (!this.subscriberMessages[subscriberId]) {
        this.$set(this.subscriberMessages, subscriberId, []);
      }

      this.subscriberMessages[subscriberId].push(message);

      // FIFO: Keep only last 50 messages per subscriber
      if (this.subscriberMessages[subscriberId].length > 50) {
        this.subscriberMessages[subscriberId].shift();
      }

      // Update message count
      const subscriber = this.subscribers.find(s => s.id === subscriberId);
      if (subscriber) {
        subscriber.messageCount++;
      }
    },

    handleSubscriberAdded(subscriber) {
      this.subscribers.push(subscriber);
      this.$set(this.subscriberMessages, subscriber.id, []);
    },

    handleSubscriberRemoved(subscriberId) {
      const index = this.subscribers.findIndex(s => s.id === subscriberId);
      if (index !== -1) {
        this.subscribers.splice(index, 1);
        this.$delete(this.subscriberMessages, subscriberId);
      }
    },

    handleQueueMessageAdded(queueMessage) {
      // Only handle PubSub topic messages
      if (queueMessage.queueName === 'playground/pubsub/') {
        this.queueMessages.push(queueMessage);
      }
    },

    handleQueueMessageRemoved(data) {
      // Remove message from queue display
      if (data.queueName === 'playground/pubsub/') {
        const index = this.queueMessages.findIndex(m => m.id === data.messageId);
        if (index !== -1) {
          this.queueMessages.splice(index, 1);
        }
      }
    },

    async publishMessage(content) {
      this.isLoading = true;
      this.error = '';

      try {
        const response = await fetch('/api/pubsub/publish', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ content })
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to publish message');
        }
      } catch (err) {
        console.error('Error publishing message:', err);
        this.error = err.message || 'Failed to publish message';
      } finally {
        this.isLoading = false;
      }
    },

    async addSubscriber() {
      this.isLoading = true;
      this.error = '';

      try {
        const response = await fetch('/api/pubsub/subscribers', {
          method: 'POST'
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to add subscriber');
        }
        // Subscriber added via SignalR event
      } catch (err) {
        console.error('Error adding subscriber:', err);
        this.error = err.message || 'Failed to add subscriber';
      } finally {
        this.isLoading = false;
      }
    },

    async removeLastSubscriber() {
      if (this.subscribers.length <= 2) return;

      const lastSubscriber = this.subscribers[this.subscribers.length - 1];
      this.isLoading = true;
      this.error = '';

      try {
        const response = await fetch(`/api/pubsub/subscribers/${lastSubscriber.id}`, {
          method: 'DELETE'
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to remove subscriber');
        }
        // Subscriber removed via SignalR event
      } catch (err) {
        console.error('Error removing subscriber:', err);
        this.error = err.message || 'Failed to remove subscriber';
      } finally {
        this.isLoading = false;
      }
    },

    async clearMessages() {
      try {
        await fetch('/api/pubsub/clear', { method: 'POST' });
        // Clear local message history
        Object.keys(this.subscriberMessages).forEach(id => {
          this.subscriberMessages[id] = [];
        });
        this.subscribers.forEach(s => s.messageCount = 0);
        this.queueMessages = [];
        this.error = '';
      } catch (err) {
        console.error('Error clearing messages:', err);
      }
    }
  }
};
</script>

<style scoped>
.pubsub-content {
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

.publisher {
  background-color: #e8f5e9;
  border-color: #a5d6a7;
}

.subscribers {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 400px;
  overflow-y: auto;
}

.subscriber {
  background-color: #fff3e0;
  border-color: #ffcc80;
}

.subscriber.inactive {
  opacity: 0.6;
}

.subscriber-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.message-count {
  font-size: 12px;
  color: #757575;
}
</style>
