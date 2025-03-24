using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

    class Program
    {
        // Metoda wizualizacji – pozostawiamy ją, ale wywołanie zostanie zakomentowane
        static void Draw(Building building, Elevator elevator)
        {
            Console.Clear();
            for (int floor = building.TotalFloors - 1; floor >= 0; floor--)
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
                if (building.WaitingPassengers[floor].Count > 0)
                {
                    floorDisplay += $"Oczekuje: {building.WaitingPassengers[floor].Count}";
                }

                Console.WriteLine(floorDisplay);
            }
            Console.WriteLine("Docelowe piętra pasażerów w windzie: " +
                              (elevator.Passengers.Count > 0 ? string.Join(", ", elevator.Passengers.Select(p => p.DestinationFloor)) : "Brak"));
        }

        // Funkcja wybierająca docelowe piętro na podstawie wybranego algorytmu sterowania
        static int? GetTargetFloor(Building building, Elevator elevator, ControlAlgorithm algorithm)
        {
            if (algorithm == ControlAlgorithm.Basic)
            {
                int? target = elevator.GetNextDestination();
                if (!target.HasValue)
                    target = building.GetNearestWaitingFloor(elevator.CurrentFloor);
                return target;
            }
            else if (algorithm == ControlAlgorithm.Directional)
            {
                // Jeśli winda jest bezczynna, wybieramy kierunek na podstawie oczekujących pasażerów
                if (elevator.ElevatorDirection == Direction.Idle)
                {
                    int? up = building.GetNearestWaitingFloorAbove(elevator.CurrentFloor);
                    int? down = building.GetNearestWaitingFloorBelow(elevator.CurrentFloor);
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
                    int? nextWaiting = building.GetNearestWaitingFloorAbove(elevator.CurrentFloor);
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
                        target = building.GetNearestWaitingFloorBelow(elevator.CurrentFloor) ?? elevator.GetNextDestinationBelow();
                    }
                    return target;
                }
                else if (elevator.ElevatorDirection == Direction.Down)
                {
                    int? nextWaiting = building.GetNearestWaitingFloorBelow(elevator.CurrentFloor);
                    int? nextDestination = elevator.GetNextDestinationBelow();
                    int? target = null;
                    if (nextWaiting.HasValue && nextDestination.HasValue)
                        target = Math.Max(nextWaiting.Value, nextDestination.Value);
                    else
                        target = nextWaiting ?? nextDestination;

                    if (!target.HasValue)
                    {
                        elevator.ElevatorDirection = Direction.Up;
                        target = building.GetNearestWaitingFloorAbove(elevator.CurrentFloor) ?? elevator.GetNextDestinationAbove();
                    }
                    return target;
                }
            }
            return null;
        }

        static void Main(string[] args)
        {
            int totalFloors = 10;
            int elevatorCapacity = 4;
            Building building = new Building(totalFloors);
            Elevator elevator = new Elevator(0, elevatorCapacity);
            Random random = new Random();

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
                building.AddWaitingPassenger(new Passenger(start, dest, currentTime));
            }

            // Główna pętla symulacji – symulujemy do momentu obsłużenia 1000 pasażerów
            while (servedPassengersCount < 10000)
            {
                currentTime++;

                // Losowo dodajemy nowych pasażerów
                if (random.NextDouble() < 0.3)
                {
                    int start = random.Next(0, totalFloors);
                    int dest = random.Next(0, totalFloors);
                    if (dest == start)
                    {
                        dest = (start + 1) % totalFloors;
                    }
                    building.AddWaitingPassenger(new Passenger(start, dest, currentTime));
                }

                // Pasażerowie wysiadają, jeśli dotarli do celu
                elevator.DropOffPassengers();

                // Pasażerowie wchodzą, gdy winda znajduje się na ich piętrze.
                // Dla każdego, który wsiądzie, obliczamy czas oczekiwania.
                List<Passenger> waitingPassengers = building.GetWaitingPassengersAtFloor(elevator.CurrentFloor);
                foreach (var p in waitingPassengers)
                {
                    if (elevator.Passengers.Count < elevator.Capacity)
                    {
                        int waitTime = currentTime - p.RequestTime;
                        totalWaitingTime += waitTime;
                        servedPassengersCount++;
                        elevator.Passengers.Add(p);
                    }
                    else
                    {
                        // Jeśli winda jest pełna, pasażerowie pozostają na piętrze
                        break;
                    }
                }
                building.ClearWaitingPassengersAtFloor(elevator.CurrentFloor);

                // Wyznaczamy następny cel w zależności od wybranego algorytmu
                int? targetFloor = GetTargetFloor(building, elevator, selectedAlgorithm);

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
                Draw(building, elevator);
                Thread.Sleep(1000);
            }

            double averageWaitingTime = totalWaitingTime / servedPassengersCount;
            double averageDistancePerPassenger = (double)totalDistance / servedPassengersCount;

            Console.WriteLine("Symulacja zakończona po obsłużeniu 1000 pasażerów.");
            Console.WriteLine($"Wybrany algorytm sterowania: {selectedAlgorithm}");
            Console.WriteLine($"Średni czas oczekiwania: {averageWaitingTime:F2} sekundy");
            Console.WriteLine($"Średnia droga pokonana przez windę: {averageDistancePerPassenger:F2} pięter na pasażera");
        }
    }
}
