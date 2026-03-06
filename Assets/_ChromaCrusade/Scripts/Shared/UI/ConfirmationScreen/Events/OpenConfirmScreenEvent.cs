using System;

public struct OpenConfirmScreenEvent
{
    public string message;
    public Action action;
    public NavItem yesNavItem;
    public NavItem noNavItem;
}
