import { Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Link } from "react-router-dom";
import WeekendIcon from '@mui/icons-material/Weekend';
import { navigationConfig } from "./NavigationConfig";



export function SideNavigationComponent({ width = 200, setRoomDialogOpen }: { width?: number, setRoomDialogOpen: (i: boolean) => void }) {
  return (<Drawer
    sx={{
      width,
      flexShrink: 0,
      '& .MuiDrawer-paper': {
        width,
        boxSizing: 'border-box',
      },
    }}
    variant="permanent"
    anchor="left"
    >
    <List>
            {navigationConfig.map((nav, index) => (
              <ListItem key={index}>
                <ListItemButton component={Link} to={nav.route}>
                  <ListItemIcon>
                    <nav.icon />
                  </ListItemIcon>
                  <ListItemText primary={nav.label} />
                </ListItemButton>
              </ListItem>
            ))}
            <ListItem key={"room-nav"}>
                <ListItemButton onClick={() => setRoomDialogOpen(true)}>
                  <ListItemIcon>
                    <WeekendIcon/>
                  </ListItemIcon>
                  <ListItemText primary={"Room"} />
                </ListItemButton>
              </ListItem>
          </List>
    </Drawer>)
}