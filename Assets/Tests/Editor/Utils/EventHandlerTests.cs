using NUnit.Framework;
using UnityEngine;
using MCUU.Events;

namespace Tests {
  public class EventListener : IEventListener {
    private string eventName;
    private string eventValue;

    public EventListener(string eventName) {
      this.eventName = eventName;
      this.eventValue = null;
    }

    public string GetEventValue() {
      return eventValue;
    }

    public void OnEvent(string eventName, string eventValue) {
      if (this.eventName == eventName) {
        this.eventValue = eventValue;
      }
    }
  }

  public class EventHandlerTests {
    [Test]
    public void TestNotifyEventListeners() {
      EventListener eventListener1 = new EventListener("event1");
      EventListener eventListener2 = new EventListener("event2");

      EventHandler.GetSingleton().RegisterEventListener(eventListener1);
      EventHandler.GetSingleton().RegisterEventListener(eventListener2);
      EventHandler.GetSingleton().NotifyEventListeners("event1", "value1");

      Assert.AreEqual("value1", eventListener1.GetEventValue());
      Assert.AreEqual(null, eventListener2.GetEventValue());
    }
  }
}
