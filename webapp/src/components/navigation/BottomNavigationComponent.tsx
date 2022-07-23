import { BottomNavigation, BottomNavigationAction, ListItemIcon, ListItemText, Menu, MenuItem } from "@mui/material";
import { Link } from "react-router-dom";
import MenuIcon from '@mui/icons-material/Menu';
import WeekendIcon from '@mui/icons-material/Weekend';

import './BottomNavigationComponent.css';
import { navigationConfig } from "./NavigationConfig";
import { useState } from "react";

export function BottomNavigationComponent({ setRoomDialogOpen }: {setRoomDialogOpen: (i: boolean) => void}) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };

  return (<><BottomNavigation showLabels className="bottom-navigation-menu">
      {navigationConfig.filter(x => !x.mobileHidden).map((x, i) => 
      <BottomNavigationAction key={i} label={x.label} component={Link} to={x.route} icon={<x.icon />} />)}
      <BottomNavigationAction icon={<MenuIcon />} onClick={handleClick} label="More" />
    </BottomNavigation>
    <Menu
        id="more-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        MenuListProps={{
          'aria-labelledby': 'basic-button',
        }}
      >
      {navigationConfig.filter(x => x.mobileHidden).map((x, i) => {
        return <MenuItem key={i} component={Link} onClick={handleClose} to={x.route}>
        <ListItemIcon><x.icon /></ListItemIcon>
        <ListItemText>{x.label}</ListItemText>
      </MenuItem>
      }
        )}
        <MenuItem key={"room-nav"} onClick={() => setRoomDialogOpen(true)}>
              <ListItemIcon>
                <WeekendIcon/>
              </ListItemIcon>
              <ListItemText primary={"Room"} />
        </MenuItem>
      </Menu>
    </>)
}