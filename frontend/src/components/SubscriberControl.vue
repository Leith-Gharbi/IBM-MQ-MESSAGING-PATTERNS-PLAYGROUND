<template>
  <div class="subscriber-control">
    <span class="subscriber-count">{{ subscriberCount }} subscribers</span>
    <div class="buttons">
      <button
        class="add-btn"
        @click="$emit('add')"
        :disabled="subscriberCount >= maxSubscribers || disabled"
        title="Add subscriber"
      >
        +
      </button>
      <button
        class="remove-btn"
        @click="$emit('remove')"
        :disabled="subscriberCount <= minSubscribers || disabled"
        title="Remove subscriber"
      >
        -
      </button>
    </div>
  </div>
</template>

<script>
/**
 * SubscriberControl component
 *
 * Add/Remove subscriber buttons with count display.
 *
 * @prop {Number} subscriberCount - Current number of subscribers
 * @prop {Number} minSubscribers - Minimum required (default 2)
 * @prop {Number} maxSubscribers - Maximum allowed (default 4)
 * @prop {Boolean} disabled - Disable buttons
 *
 * @emits add - When add button clicked
 * @emits remove - When remove button clicked
 */
export default {
  name: 'SubscriberControl',

  props: {
    subscriberCount: {
      type: Number,
      required: true
    },
    minSubscribers: {
      type: Number,
      default: 2
    },
    maxSubscribers: {
      type: Number,
      default: 4
    },
    disabled: {
      type: Boolean,
      default: false
    }
  }
};
</script>

<style scoped>
.subscriber-control {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background-color: #e3f2fd;
  border-radius: 4px;
}

.subscriber-count {
  font-size: 14px;
  color: #1565c0;
  font-weight: 500;
}

.buttons {
  display: flex;
  gap: 4px;
}

.add-btn,
.remove-btn {
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 50%;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.add-btn {
  background-color: #4caf50;
  color: white;
}

.add-btn:hover:not(:disabled) {
  background-color: #388e3c;
}

.remove-btn {
  background-color: #f44336;
  color: white;
}

.remove-btn:hover:not(:disabled) {
  background-color: #d32f2f;
}

.add-btn:disabled,
.remove-btn:disabled {
  background-color: #bdbdbd;
  cursor: not-allowed;
}
</style>
