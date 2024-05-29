using Microsoft.AspNetCore.Mvc;

internal readonly record struct WaterHeaterInput(
  [FromForm] string DeviceText, // *MAC address*
  [FromForm] string Password, // *Random string*
  [FromForm] string ModuleApi, // 1.5
  [FromForm] string ModFwVer, // 3.1
  [FromForm] string MasterFwVer, // 06.03
  [FromForm] string MasterModelId, // B1.00
  [FromForm] string DisplayFwVer, // 03.04
  [FromForm] string WifiFwVer, // C2.4.0.3.AO7
  [FromForm] int UpdateRate, // 300
  [FromForm] string Mode, // EnergySmart
  [FromForm] int SetPoint, // 120
  [FromForm] string Units, // F
  [FromForm] string LeakDetect, // NotDetected
  [FromForm] int MaxSetPoint, // 120
  [FromForm] string Grid, // Enabled
  [FromForm] string AirFilterStatus, // OK
  [FromForm] bool CondensePumpFail, // False
  [FromForm] string AvailableModes, // Standard,Vacation,EnergySmart
  [FromForm] bool SystemInHeating, // False
  [FromForm] string HotWaterVol, // High
  [FromForm] string Leak, // None
  [FromForm] string DryFire, // None
  [FromForm] string ElementFail, // None
  [FromForm] string TankSensorFail, // None
  [FromForm] bool EcoError, // False
  [FromForm] string MasterDispFail, // None
  [FromForm] string CompSensorFail, // None
  [FromForm] string SysSensorFail, // None
  [FromForm] string SystemFail, // None
  [FromForm] int UpperTemp, // 122
  [FromForm] int LowerTemp, // 104
  [FromForm] string FaultCodes, // 0
  [FromForm] string UnConnectNumber, // 0
  [FromForm] string AddrData, // *Two strings*
  [FromForm] string SignalStrength // -46
);