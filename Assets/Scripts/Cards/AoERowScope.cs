using UnityEngine;

/// <summary>
/// Scope for Area-of-Effect targeting with rows.
/// BothRows: hits enemies in both rows (no row filtering)
/// SelectedRow: hits only enemies in the same row as the selected target (for 4–5 enemies); for ≤3, treats as BothRows
/// </summary>
public enum AoERowScope
{
	BothRows = 0,
	SelectedRow = 1
}



