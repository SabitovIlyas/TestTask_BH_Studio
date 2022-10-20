public interface Subject
{
    public void RegisterObserver(Observer observer);
    public void RemoveObserver(Observer observer);
    public void NotifyObservers(Event customEvent);
}