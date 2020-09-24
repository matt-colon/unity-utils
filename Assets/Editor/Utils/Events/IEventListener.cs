namespace MCUU.Events {
  /// <summary>
  /// The interface for the event listener interface used by with <see cref="EventListener"/>.
  /// </summary>
  public interface IEventListener {
    /// <summary>
    /// Notifies the event listener of an event that has occurred.
    /// </summary>
    /// <param name="eventName">
    /// The name of the event.
    /// </param>
    /// <param name="eventValue">
    /// The value of the event represented as a string.
    /// </param>
    void OnEvent(string eventName, string eventValue);
  }
}
