/**
 * SignalR service module for real-time MQ messaging.
 *
 * WHY: This service centralizes SignalR connection management,
 * providing auto-reconnect and clean separation from Vue components.
 * All MQ events are received through this single connection.
 */

import * as signalR from '@microsoft/signalr';

let connection = null;

/**
 * Creates a new SignalR connection to the MessageHub.
 * @param {string} hubUrl - The URL of the SignalR hub (default: '/messageHub')
 * @returns {HubConnection} The SignalR connection instance
 */
export function createConnection(hubUrl = '/messageHub') {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return connection;
}

/**
 * Gets the current SignalR connection instance.
 * @returns {HubConnection|null} The connection or null if not created
 */
export function getConnection() {
  return connection;
}

/**
 * Starts the SignalR connection and joins the session.
 * @returns {Promise<Object>} Session info with connection status and subscribers
 */
export async function startConnection() {
  if (!connection) {
    throw new Error('Connection not created. Call createConnection first.');
  }

  await connection.start();
  const sessionInfo = await connection.invoke('JoinSession');
  return sessionInfo;
}

/**
 * Stops the SignalR connection gracefully.
 */
export async function stopConnection() {
  if (connection) {
    try {
      await connection.invoke('LeaveSession');
    } catch {
      // Ignore errors when leaving session
    }
    await connection.stop();
  }
}

/**
 * Registers an event handler for server-to-client messages.
 * @param {string} eventName - The event name to listen for
 * @param {Function} handler - The callback function
 */
export function on(eventName, handler) {
  if (connection) {
    connection.on(eventName, handler);
  }
}

/**
 * Removes an event handler.
 * @param {string} eventName - The event name to stop listening for
 * @param {Function} handler - The callback function to remove
 */
export function off(eventName, handler) {
  if (connection) {
    connection.off(eventName, handler);
  }
}

export default {
  createConnection,
  getConnection,
  startConnection,
  stopConnection,
  on,
  off
};
