/// <summary>
/// A UI panel that can be folded (collapsed) to a minimal state and unfolded again.
/// Panels implementing this participate in the Shortcuts panel's "Fold All / Unfold All" action.
/// </summary>
public interface IFoldable
{
    // Fold (true) the panel to its minimal state, or unfold (false) it to the full view.
    void SetFolded(bool folded);
}
