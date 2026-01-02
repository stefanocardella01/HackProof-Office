using UnityEngine;


/// <summary>
/// Interfaccia base per tutte le schermate del PC
/// </summary>

public interface IPCScreen
{
    void Show();
    void Hide();
    void Initialize(PCInterfaceManager manager);

}
