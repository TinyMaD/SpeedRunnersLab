export function goToUserProfile($router, platformID) {
  if (!platformID) return;
  $router.push(`/profile/${platformID}`);
}