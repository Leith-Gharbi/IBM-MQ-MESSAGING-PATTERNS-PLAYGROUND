<template>
  <div class="message-input">
    <input
      type="text"
      v-model="message"
      :placeholder="placeholder"
      :disabled="disabled"
      @keyup.enter="sendMessage"
    />
    <button
      @click="sendMessage"
      :disabled="disabled || !message.trim()"
    >
      {{ buttonText }}
    </button>
  </div>
</template>

<script>
/**
 * MessageInput component
 *
 * Text input with send button for submitting messages.
 *
 * @prop {String} placeholder - Input placeholder text
 * @prop {String} buttonText - Text for the send button
 * @prop {Boolean} disabled - Whether input is disabled
 *
 * @emits send - Emits the message content when send is clicked
 */
export default {
  name: 'MessageInput',

  props: {
    placeholder: {
      type: String,
      default: 'Type a message...'
    },
    buttonText: {
      type: String,
      default: 'Send'
    },
    disabled: {
      type: Boolean,
      default: false
    }
  },

  data() {
    return {
      message: ''
    };
  },

  methods: {
    sendMessage() {
      const content = this.message.trim();
      if (content) {
        this.$emit('send', content);
        this.message = '';
      }
    }
  }
};
</script>

<style scoped>
.message-input {
  display: flex;
  gap: 8px;
  padding: 12px;
  background-color: #f5f5f5;
  border-radius: 4px;
}

input {
  flex: 1;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

input:focus {
  outline: none;
  border-color: #2196f3;
}

input:disabled {
  background-color: #eee;
  cursor: not-allowed;
}

button {
  padding: 8px 16px;
  background-color: #2196f3;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

button:hover:not(:disabled) {
  background-color: #1976d2;
}

button:disabled {
  background-color: #bdbdbd;
  cursor: not-allowed;
}
</style>
