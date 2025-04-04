using System;

namespace ElevatorSimulation
{
    enum Direction
    {
        Up,
        Down,
        Idle
    }
    enum ControlAlgorithm
    {
        Basic,       // Algorytm: wybór najbliższego celu (pasażer wewnątrz windy lub oczekujący)
        Directional  // Algorytm: utrzymanie kierunku jazdy (najpierw sprawdzamy piętra w danym kierunku)
    }
    class Building
    {
        public int totalFloors;
        Elevator elevator;
        Random random;
        public List<Passenger>[] WaitingPassengers { get; set; }

        public Building(int newTotalFloors)
        {
            totalFloors = newTotalFloors;
            elevator = new Elevator(0, 4);

            WaitingPassengers = new List<Passenger>[totalFloors];
            for (int i = 0; i < totalFloors; i++)
            {
                WaitingPassengers[i] = new List<Passenger>();
            }

            random = new Random();
        }

        public void Solve()
        {
            // Wybór algorytmu sterowania – zmień wartość, aby przetestować inny algorytm:
            // ControlAlgorithm selectedAlgorithm = ControlAlgorithm.Basic;
            ControlAlgorithm selectedAlgorithm = ControlAlgorithm.Directional;

            // Parametry symulacji
            int servedPassengersCount = 0;
            double totalWaitingTime = 0; // suma czasu oczekiwania dla obsłużonych pasażerów
            int totalDistance = 0;       // suma pięter, jakie przebyła winda
            int currentTime = 0;         // symulowany czas (w sekundach)

            // Dodajemy początkowych pasażerów (np. 5) – RequestTime = 0
            for (int i = 0; i < 5; i++)
            {
                int start = random.Next(0, totalFloors);
                int dest = random.Next(0, totalFloors);
                while (dest == start)
                {
                    dest = random.Next(0, totalFloors);
                }
                AddWaitingPassenger(new Passenger(start, dest, currentTime));
            }

            // Główna pętla symulacji – symulujemy do momentu obsłużenia 1000 pasażerów
            while (servedPassengersCount < 1000)
            {
                currentTime+=5;

                // Losowo dodajemy nowych pasażerów
                if (random.NextDouble() < 0.3)
                {
                    int start = random.Next(0, totalFloors);
                    int dest = random.Next(0, totalFloors);
                    if (dest == start)
                    {
                        dest = (start + 1) % totalFloors;
                    }
                    AddWaitingPassenger(new Passenger(start, dest, currentTime));
                }

                // Doliczamy czas jeżeli musimy otworzyć drzwi windy na piętrze
                if(elevator.Passengers.Where(p => p.DestinationFloor == elevator.CurrentFloor).Count() != 0 || (WaitingPassengers[elevator.CurrentFloor].Any())){
                    Thread.Sleep(1000);
                    currentTime += 10;
                }

                // Pasażerowie wysiadają, jeśli dotarli do celu
                if(elevator.Passengers.Where(p => p.DestinationFloor == elevator.CurrentFloor).Count() != 0)
                {
                    elevator.DropOffPassengers();
                }


                // Pasażerowie wchodzą, gdy winda znajduje się na ich piętrze.
                if (WaitingPassengers[elevator.CurrentFloor].Any())
                {
                    // Dla każdego, który wsiądzie, obliczamy czas oczekiwania.
                    List<Passenger> waitingPassengersOnFloor = GetWaitingPassengersAtFloor(elevator.CurrentFloor);
                    var passengersToRemove = new List<Passenger>();

                    foreach (var p in waitingPassengersOnFloor.ToList()) // Tworzymy kopię listy, aby unikać modyfikacji podczas iteracji
                    {
                        if (elevator.Passengers.Count < elevator.Capacity)
                        {
                            int waitTime = currentTime - p.RequestTime;
                            totalWaitingTime += waitTime;
                            servedPassengersCount++;
                            elevator.Passengers.Add(p);
                            passengersToRemove.Add(p); // Oznaczamy pasażera do usunięcia
                        }
                        else
                        {
                            break; // Winda jest pełna, reszta pasażerów zostaje na piętrze
                        }
                    }
                    // Usunięcie tylko tych pasażerów, którzy weszli do windy
                    WaitingPassengers[elevator.CurrentFloor].RemoveAll(p => passengersToRemove.Contains(p));
                }
                

                // Wyznaczamy następny cel w zależności od wybranego algorytmu
                int? targetFloor = GetTargetFloor(elevator, selectedAlgorithm);

                int previousFloor = elevator.CurrentFloor;
                if (targetFloor.HasValue)
                {
                    if (elevator.CurrentFloor < targetFloor.Value)
                    {
                        elevator.CurrentFloor++;
                        elevator.ElevatorDirection = Direction.Up;
                    }
                    else if (elevator.CurrentFloor > targetFloor.Value)
                    {
                        elevator.CurrentFloor--;
                        elevator.ElevatorDirection = Direction.Down;
                    }
                }
                else
                {
                    elevator.ElevatorDirection = Direction.Idle;
                }
                totalDistance += Math.Abs(elevator.CurrentFloor - previousFloor);

                // WIZUALIZACJA
                Draw(elevator);
                Thread.Sleep(500);
            }

            double averageWaitingTime = totalWaitingTime / servedPassengersCount;
            double averageDistancePerPassenger = (double)totalDistance / servedPassengersCount;

            Console.WriteLine("Symulacja zakończona po obsłużeniu 1000 pasażerów.");
            Console.WriteLine($"Wybrany algorytm sterowania: {selectedAlgorithm}");
            Console.WriteLine($"Średni czas oczekiwania: {averageWaitingTime:F2} sekundy");
            Console.WriteLine($"Średnia droga pokonana przez windę: {averageDistancePerPassenger:F2} pięter na pasażera");
        }
        void Draw(Elevator elevator)
        {
            Console.Clear();
            for (int floor = totalFloors - 1; floor >= 0; floor--)
            {
                string floorDisplay = $"Piętro {floor}: ";

                // Jeżeli winda jest na tym piętrze
                if (elevator.CurrentFloor == floor)
                {
                    floorDisplay += "[Winda ";
                    floorDisplay += $"{elevator.Passengers.Count} os.";
                    floorDisplay += "] ";
                }
                else
                {
                    floorDisplay += "           ";
                }

                // Wyświetlamy liczbę oczekujących pasażerów
                if (WaitingPassengers[floor].Count > 0)
                {
                    floorDisplay += $"Oczekuje: {WaitingPassengers[floor].Count}";
                }

                Console.WriteLine(floorDisplay);
            }
            Console.WriteLine("Docelowe piętra pasażerów w windzie: " +
                              (elevator.Passengers.Count > 0 ? string.Join(", ", elevator.Passengers.Select(p => p.DestinationFloor)) : "Brak"));
        }

        // Funkcja wybierająca docelowe piętro na podstawie wybranego algorytmu sterowania
        int? GetTargetFloor(Elevator elevator, ControlAlgorithm algorithm)
        {
            if (algorithm == ControlAlgorithm.Basic)
            {
                int? target = elevator.GetNextDestination();
                if (!target.HasValue)
                    target = GetNearestWaitingFloor(elevator.CurrentFloor);
                return target;
            }
            else if (algorithm == ControlAlgorithm.Directional)
            {
                // Jeśli winda jest bezczynna, wybieramy kierunek na podstawie oczekujących pasażerów
                if (elevator.ElevatorDirection == Direction.Idle)
                {
                    int? up = GetNearestWaitingFloorAbove(elevator.CurrentFloor);
                    int? down = GetNearestWaitingFloorBelow(elevator.CurrentFloor);
                    if (up.HasValue)
                    {
                        elevator.ElevatorDirection = Direction.Up;
                        return up;
                    }
                    else if (down.HasValue)
                    {
                        elevator.ElevatorDirection = Direction.Down;
                        return down;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (elevator.ElevatorDirection == Direction.Up)
                {
                    // Szukamy celu powyżej: zarówno pasażerów w windzie, jak i oczekujących
                    int? nextWaiting = GetNearestWaitingFloorAbove(elevator.CurrentFloor);
                    int? nextDestination = elevator.GetNextDestinationAbove();
                    int? target = null;
                    if (nextWaiting.HasValue && nextDestination.HasValue)
                        target = Math.Min(nextWaiting.Value, nextDestination.Value);
                    else
                        target = nextWaiting ?? nextDestination;

                    // Jeśli nic nie ma powyżej, zmieniamy kierunek
                    if (!target.HasValue)
                    {
                        elevator.ElevatorDirection = Direction.Down;
                        target = GetNearestWaitingFloorBelow(elevator.CurrentFloor) ?? elevator.GetNextDestinationBelow();
                    }
                    return target;
                }
                else if (elevator.ElevatorDirection == Direction.Down)
                {
                    int? nextWaiting = GetNearestWaitingFloorBelow(elevator.CurrentFloor);
                    int? nextDestination = elevator.GetNextDestinationBelow();
                    int? target = null;
                    if (nextWaiting.HasValue && nextDestination.HasValue)
                        target = Math.Max(nextWaiting.Value, nextDestination.Value);
                    else
                        target = nextWaiting ?? nextDestination;

                    if (!target.HasValue)
                    {
                        elevator.ElevatorDirection = Direction.Up;
                        target = GetNearestWaitingFloorAbove(elevator.CurrentFloor) ?? elevator.GetNextDestinationAbove();
                    }
                    return target;
                }
            }
            return null;
        }

        public void AddWaitingPassenger(Passenger p)
        {
            if (p.StartFloor >= 0 && p.StartFloor < totalFloors)
            {
                WaitingPassengers[p.StartFloor].Add(p);
            }
        }

        public List<Passenger> GetWaitingPassengersAtFloor(int floor)
        {
            if (floor >= 0 && floor < totalFloors)
                return new List<Passenger>(WaitingPassengers[floor]);
            return new List<Passenger>();
        }

        public void ClearWaitingPassengersAtFloor(int floor)
        {
            if (floor >= 0 && floor < totalFloors)
                WaitingPassengers[floor].Clear();
        }

        // Dla algorytmu podstawowego: najbliższe piętro (w dowolnym kierunku) z oczekującymi pasażerami
        public int? GetNearestWaitingFloor(int currentFloor)
        {
            int? nearestFloor = null;
            int minDiff = int.MaxValue;
            for (int i = 0; i < totalFloors; i++)
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
            for (int i = currentFloor + 1; i < totalFloors; i++)
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