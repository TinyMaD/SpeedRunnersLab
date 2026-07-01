-- Migration for demand-driven Steam refresh scheduling.

SET @schema_name = DATABASE();

SET @column_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND COLUMN_NAME = 'PlayTimeModifyTime'
);

SET @sql = IF(
  @column_exists = 0,
  'ALTER TABLE `RankInfo` ADD COLUMN `PlayTimeModifyTime` datetime NULL DEFAULT NULL COMMENT ''play time refresh time'' AFTER `PlayTime`',
  'SELECT ''RankInfo.PlayTimeModifyTime already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @column_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND COLUMN_NAME = 'LastProfileViewTime'
);

SET @sql = IF(
  @column_exists = 0,
  'ALTER TABLE `RankInfo` ADD COLUMN `LastProfileViewTime` datetime NULL DEFAULT NULL COMMENT ''last profile view time'' AFTER `PlayTimeModifyTime`',
  'SELECT ''RankInfo.LastProfileViewTime already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

UPDATE `RankInfo`
SET `PlayTimeModifyTime` = COALESCE(`PlayTimeModifyTime`, `ModifyTime`, `CreateTime`)
WHERE `PlayTimeModifyTime` IS NULL;

SET @index_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND INDEX_NAME = 'idx_rankinfo_platformid'
);

SET @sql = IF(
  @index_exists = 0,
  'ALTER TABLE `RankInfo` ADD INDEX `idx_rankinfo_platformid` (`PlatformID`) USING BTREE',
  'SELECT ''idx_rankinfo_platformid already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND INDEX_NAME = 'idx_rankinfo_modify_time'
);

SET @sql = IF(
  @index_exists = 0,
  'ALTER TABLE `RankInfo` ADD INDEX `idx_rankinfo_modify_time` (`ModifyTime`) USING BTREE',
  'SELECT ''idx_rankinfo_modify_time already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND INDEX_NAME = 'idx_rankinfo_playtime_modify_time'
);

SET @sql = IF(
  @index_exists = 0,
  'ALTER TABLE `RankInfo` ADD INDEX `idx_rankinfo_playtime_modify_time` (`PlayTimeModifyTime`) USING BTREE',
  'SELECT ''idx_rankinfo_playtime_modify_time already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @index_exists = (
  SELECT COUNT(*)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = 'RankInfo'
    AND INDEX_NAME = 'idx_rankinfo_profile_view_time'
);

SET @sql = IF(
  @index_exists = 0,
  'ALTER TABLE `RankInfo` ADD INDEX `idx_rankinfo_profile_view_time` (`LastProfileViewTime`) USING BTREE',
  'SELECT ''idx_rankinfo_profile_view_time already exists'''
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
