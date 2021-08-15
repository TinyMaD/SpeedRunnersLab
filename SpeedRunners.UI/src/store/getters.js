const getters = {
  sidebar: state => state.app.sidebar,
  device: state => state.app.device,
  steamId: state => state.user.steamId,
  avatar: state => state.user.avatar,
  name: state => state.user.name,
  rankType: state => state.user.rankType,
  participate: state => state.user.participate,
  permission_routes: state => state.permission.routes
};
export default getters;