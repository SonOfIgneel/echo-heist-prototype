using System.Collections.Generic;
using UnityEngine;

namespace EchoHeist
{
    public sealed class PressurePlateActivator : MonoBehaviour
    {
        private readonly HashSet<PressurePlate> _occupiedPlates = new HashSet<PressurePlate>();

        internal void RegisterPlate(PressurePlate plate)
        {
            if (plate != null) _occupiedPlates.Add(plate);
        }

        internal void ForgetPlate(PressurePlate plate) => _occupiedPlates.Remove(plate);

        private void OnDisable() => ReleaseAllPlates();
        private void OnDestroy() => ReleaseAllPlates();

public void ReleaseAllPlates()
        {
            if (_occupiedPlates.Count == 0) return;

            var plates = new PressurePlate[_occupiedPlates.Count];
            _occupiedPlates.CopyTo(plates);
            _occupiedPlates.Clear();

            foreach (PressurePlate plate in plates)
            {
                if (plate != null) plate.Unregister(this);
            }
        }
    }
}
