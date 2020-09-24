using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MCUU.Events {
  /// <summary>
  /// Handles the registration and notification of event listeners.
  /// </summary>
  public class EventHandler {
    /// <summary>
    /// Singleton instance of the event handler.
    /// </summary>
    private static EventHandler eventHandler;
    
    /// <summary>
    /// Contains all registered event listeners.
    /// </summary>
    private List<IEventListener> eventListeners;

    /// <summary>
    /// Event handler constructor.
    /// </summary>
    private EventHandler() {
      eventListeners = new List<IEventListener>();
    }

    /// <summary>
    /// Accessor for the event handler singleton.
    /// </summary>
    /// <returns>
    /// The event handler singleton.
    /// </returns>
    public static EventHandler GetSingleton() {
      if (eventHandler == null) {
        eventHandler = new EventHandler();
      }
      return eventHandler;
    }

    /// <summary>
    /// Registers an event listener with the event handler.
    /// </summary>
    /// <param name="eventListener">
    /// The event listener to register.
    /// </param>
    public void RegisterEventListener(IEventListener eventListener) {
      eventListeners.Add(eventListener);
    }

    /// <summary>
    /// Unregisters an event listener from the event handler.
    /// </summary>
    /// <param name="eventListener">
    /// The event listener to unregister.
    /// </param>
    public void UnregisterEventListener(IEventListener eventListener) {
      eventListeners.Remove(eventListener);
    }

    /// <summary>
    /// Notifies the event listeners of an event that has occurred.
    /// </summary>
    /// <param name="eventName">
    /// The name of the event.
    /// </param>
    /// <param name="eventValue">
    /// The value of the event represented as a string.
    /// </param>
    public void NotifyEventListeners(string eventName, string eventValue) {
      foreach (IEventListener eventListener in eventListeners) {
        eventListener.OnEvent(eventName, eventValue);
      }
    }
  }
}
