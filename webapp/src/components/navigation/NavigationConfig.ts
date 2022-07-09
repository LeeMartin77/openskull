import { Queue, Home, List } from "@mui/icons-material";

export const navigationConfig = [
  {
    label: "Home",
    route: "/",
    icon: Home,
    mobileHidden: false
  },
  {
    label: "Queues",
    route: "/queue",
    icon: Queue,
    mobileHidden: false
  },
  {
    label: "Game List",
    route: "/games",
    icon: List,
    mobileHidden: false
  },
]