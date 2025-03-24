namespace ElevatorSimulation
{
    class Building
    {
        public int TotalFloors { get; set; }
        public List<Passenger>[] WaitingPassengers { get; set; }

        public Building(int totalFloors)
        {
            TotalFloors = totalFloors;
            WaitingPassengers = new List<Passenger>[totalFloors];
            for (int i = 0; i < totalFloors; i++)
            {
                WaitingPassengers[i] = new List<Passenger>();
            }
        }

        public void AddWaitingPassenger(Passenger p)
        {
            if (p.StartFloor >= 0 && p.StartFloor < TotalFloors)
            {
                WaitingPassengers[p.StartFloor].Add(p);
            }
        }

        public List<Passenger> GetWaitingPassengersAtFloor(int floor)
        {
            if (floor >= 0 && floor < TotalFloors)
                return new List<Passenger>(WaitingPassengers[floor]);
            return new List<Passenger>();
        }

        public void ClearWaitingPassengersAtFloor(int floor)
        {
            if (floor >= 0 && floor < TotalFloors)
                WaitingPassengers[floor].Clear();
        }

        // Dla algorytmu podstawowego: najbliższe piętro (w dowolnym kierunku) z oczekującymi pasażerami
        public int? GetNearestWaitingFloor(int currentFloor)
        {
            int? nearestFloor = null;
            int minDiff = int.MaxValue;
            for (int i = 0; i < TotalFloors; i++)
            {
                if (WaitingPassengers[i].Count > 0)
                {
                    int diff = Math.Abs(currentFloor - i);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        nearestFloor = i;
                    }
                }
            }
            return nearestFloor;
        }

        // Metody dla algorytmu kierunkowego – szukamy piętra powyżej lub poniżej aktualnego
        public int? GetNearestWaitingFloorAbove(int currentFloor)
        {
            for (int i = currentFloor + 1; i < TotalFloors; i++)
            {
                if (WaitingPassengers[i].Count > 0)
                    return i;
            }
            return null;
        }

        public int? GetNearestWaitingFloorBelow(int currentFloor)
        {
            for (int i = currentFloor - 1; i >= 0; i--)
            {
                if (WaitingPassengers[i].Count > 0)
                    return i;
            }
            return null;
        }
    }
}