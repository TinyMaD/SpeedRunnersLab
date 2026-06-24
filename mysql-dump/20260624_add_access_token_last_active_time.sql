-- Migration for existing databases before deploying LastActiveTime-dependent API code.

SET @schema_name = DATABASE();

SET @column_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'AccessToken'
    AND COLUMN_NAME = 'LastActiveTime'
);

SET @sql = IF(
  @column_exists = 0,
  'ALTER TABLE `AccessToken` ADD COLUMN `LastActiveTime` datetime NULL DEFAULT NULL COMMENT ''last active time'' AFTER `LoginDate`',
  'SELECT ''AccessToken.LastActiveTime already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

UPDATE `AccessToken`
SET `LastActiveTime` = `LoginDate`
WHERE `LastActiveTime` IS NULL;

SET @index_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'AccessToken'
    AND INDEX_NAME = 'idx_platform_active'
);

SET @sql = IF(
  @index_exists = 0,
  'ALTER TABLE `AccessToken` ADD INDEX `idx_platform_active` (`PlatformID`, `LastActiveTime`) USING BTREE',
  'SELECT ''idx_platform_active already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
