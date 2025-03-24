namespace ElevatorSimulation
{
    class Elevator
    {
        public int CurrentFloor { get; set; }
        public Direction ElevatorDirection { get; set; }
        public List<Passenger> Passengers { get; set; }
        public int Capacity { get; set; }

        public Elevator(int startFloor, int capacity)
        {
            CurrentFloor = startFloor;
            Capacity = capacity;
            ElevatorDirection = Direction.Idle;
            Passengers = new List<Passenger>();
        }

        // Algorytm podstawowy: zwraca najbliższy cel wśród pasażerów wewnątrz windy
        public int? GetNextDestination()
        {
            if (Passengers.Count == 0) return null;

            int nearest = Passengers[0].DestinationFloor;
            int minDiff = Math.Abs(CurrentFloor - nearest);
            foreach (var p in Passengers)
            {
                int diff = Math.Abs(CurrentFloor - p.DestinationFloor);
                if (diff < minDiff)
                {
                    nearest = p.DestinationFloor;
                    minDiff = diff;
                }
            }
            return nearest;
        }

        // Metoda pomocnicza dla algorytmu kierunkowego: szuka najbliższego piętra powyżej, gdzie mają wysiąść pasażerowie
        public int? GetNextDestinationAbove()
        {
            var above = Passengers.Where(p => p.DestinationFloor > CurrentFloor)
                                  .Select(p => p.DestinationFloor);
            if (above.Any())
                return above.Min();
            return null;
        }

        // Podobnie dla pięter poniżej
        public int? GetNextDestinationBelow()
        {
            var below = Passengers.Where(p => p.DestinationFloor < CurrentFloor)
                                  .Select(p => p.DestinationFloor);
            if (below.Any())
                return below.Max();
            return null;
        }

        // Wysiadają pasażerowie, którzy dotarli do celu
        public void DropOffPassengers()
        {
            Passengers.RemoveAll(p => p.DestinationFloor == CurrentFloor);
        }
    }
}