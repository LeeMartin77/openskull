import { Drawer, List, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Link } from "react-router-dom";

import { navigationConfig } from "./NavigationConfig";



export function SideNavigationComponent({ width = 200 }: { width?: number }) {
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
          </List>
    </Drawer>)
}