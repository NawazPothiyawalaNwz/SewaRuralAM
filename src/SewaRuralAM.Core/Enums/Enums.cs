namespace SewaRuralAM.Core.Enums;

public enum AssetStatus
{
    Active = 1,
    UnderRepair = 2,
    Disposed = 3,
    Lost = 4,
    Damaged = 5
}

public enum PropertyDataType
{
    Text = 1,
    Number = 2,
    Date = 3,
    Boolean = 4,
    Dropdown = 5
}

public enum MovementType
{
    InitialAssignment = 1,
    Transfer = 2
}
