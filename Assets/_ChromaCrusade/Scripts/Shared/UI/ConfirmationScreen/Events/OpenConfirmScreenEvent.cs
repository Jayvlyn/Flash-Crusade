using System;

public struct OpenConfirmScreenEvent
{
    public string message;
    public Action action;
    public NavItem lastNavItem;
}
