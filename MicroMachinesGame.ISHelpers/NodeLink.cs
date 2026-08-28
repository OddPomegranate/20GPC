namespace MicroMachinesGame.ISHelpers;

public struct NodeLink(int ownerID, int neighbourID, int length)
{
	public int _ownerID = ownerID;

	public int _neighbourID = neighbourID;

	public int _length = length;
}
