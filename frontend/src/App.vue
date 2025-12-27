<template>
  <div id="app">
    <header class="app-header">
      <h1>MQ Patterns Playground</h1>
      <ConnectionStatus
        :is-connected="connectionStatus.isConnected"
        :queue-manager-name="connectionStatus.queueManagerName"
        :error-message="connectionStatus.errorMessage"
      />
    </header>

    <main class="panel-grid">
      <!-- Point-to-Point Panel (Left Column) -->
      <PointToPointPanel />

      <!-- Pub/Sub Panel (Center Column) -->
      <PubSubPanel />

      <!-- Request/Reply Panel (Right Column) -->
      <RequestReplyPanel />
    </main>
  </div>
</template>

<script>
import ConnectionStatus from './components/ConnectionStatus.vue';
import PointToPointPanel from './panels/PointToPointPanel.vue';
import PubSubPanel from './panels/PubSubPanel.vue';
import RequestReplyPanel from './panels/RequestReplyPanel.vue';
import signalrService from './services/signalrService';

/**
 * Main App component
 *
 * Provides the 3-column layout for messaging pattern panels
 * and manages the SignalR connection lifecycle.
 */
export default {
  name: 'App',

  components: {
    ConnectionStatus,
    PointToPointPanel,
    PubSubPanel,
    RequestReplyPanel
  },

  data() {
    return {
      connectionStatus: {
        isConnected: false,
        queueManagerName: '',
        errorMessage: ''
      },
      signalRState: 'Disconnected'
    };
  },

  async created() {
    await this.initializeSignalR();
  },

  beforeDestroy() {
    signalrService.stopConnection();
  },

  methods: {
    async initializeSignalR() {
      try {
        const connection = signalrService.createConnection('/messageHub');

        // Handle connection state changes
        connection.onreconnecting(() => {
          this.signalRState = 'Reconnecting';
        });

        connection.onreconnected(() => {
          this.signalRState = 'Connected';
        });

        connection.onclose(() => {
          this.signalRState = 'Disconnected';
          this.connectionStatus.isConnected = false;
        });

        // Register for MQ connection status changes
        signalrService.on('ConnectionStatusChanged', (status) => {
          this.connectionStatus = status;
        });

        // Start connection and join session
        const sessionInfo = await signalrService.startConnection();
        this.connectionStatus = sessionInfo.connectionStatus;
        this.signalRState = 'Connected';
      } catch (error) {
        console.error('Failed to connect to SignalR:', error);
        this.connectionStatus.errorMessage = 'Failed to connect to server';
      }
    }
  }
};
</script>

<style>
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  min-height: 100vh;
  background-color: #f0f2f5;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  background-color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.app-header h1 {
  font-size: 24px;
  color: #1a237e;
}

/* 3-column layout for pattern panels */
.panel-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  padding: 24px;
  max-width: 1600px;
  margin: 0 auto;
}

.panel-slot {
  min-height: 500px;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
}

.placeholder {
  color: #9e9e9e;
  font-style: italic;
}

@media (max-width: 1200px) {
  .panel-grid {
    grid-template-columns: 1fr;
  }
}
</style>
