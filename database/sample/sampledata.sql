BEGIN WORK;

INSERT INTO poudrierec2.jails (id, name, portable_name, version, architecture)
VALUES ('0a62a787-c7c1-48bc-8ba0-90d7fbe9c098', '13_0-amd64', '130-amd64', '13.0-RELEASE', 'amd64.amd64');

INSERT INTO poudrierec2.portstrees (id, name, portable_name, method, url)
VALUES
('4e6d2feb-2a99-4bed-8545-d5462c66ba0c', 'main', 'main', 'git', 'https://git.freebsd.org/ports.git');

INSERT INTO poudrierec2.portsets (id, name, portable_name)
VALUES
('11a4e47a-e778-4499-8ad3-4ad117fe0a2f', 'server', 'server'),
('14a6f67a-ed4e-462c-beb1-4d9a751ac339', 'No jobs using this so tests can delete it', 'testportstreepleaseignore'),
('47234d7c-82d1-4ffd-ae18-25aae674f245', '田中太郎', 'tanakatarou');

INSERT INTO poudrierec2.portset_members (portset, portname)
VALUES
('11a4e47a-e778-4499-8ad3-4ad117fe0a2f', 'net/rclone'),
('11a4e47a-e778-4499-8ad3-4ad117fe0a2f', 'security/sssd'),
('11a4e47a-e778-4499-8ad3-4ad117fe0a2f', 'sysutils/tmux'),
('47234d7c-82d1-4ffd-ae18-25aae674f245', 'sysutils/py-azure-cli'),
('47234d7c-82d1-4ffd-ae18-25aae674f245', 'editors/vim'),
('47234d7c-82d1-4ffd-ae18-25aae674f245', 'sysutils/htop'),
('47234d7c-82d1-4ffd-ae18-25aae674f245', 'archivers/cabextract');

INSERT INTO poudrierec2.configfiles (id, deleted, name, configtype, jail)
VALUES
('97241b1e-9c04-4b58-9cdc-4c90eef35225', false, 'Random poudriere.conf', 'poudriereconf', NULL),
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', false, 'Some make.conf', 'makeconf', '0a62a787-c7c1-48bc-8ba0-90d7fbe9c098');

INSERT INTO poudrierec2.configoptions (configfile, name, value)
VALUES
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'ZPOOL', 'zroot'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'FREEBSD_HOST', 'https://download.FreeBSD.org'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'RESOLV_CONF', '/etc/resolv.conf'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'BASEFS', '/usr/local/poudriere'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'USE_PORTLINT', 'no'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'USE_TMPFS', 'all'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'DISTFILES_CACHE', '/usr/ports/distfiles'),
('97241b1e-9c04-4b58-9cdc-4c90eef35225', 'ALLOW_MAKE_JOBS_PACKAGES', 'llvm* rust py* openjdk* mono gcc* gdal'),
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', 'DEFAULT_VERSIONS+', 'java=11 pgsql=13 php=8.0 python=3.8 python3=3.8 llvm=11 lua=5.4 samba=4.12 mono=6.8'),
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', 'NO_MODULES', 'yes');

INSERT INTO poudrierec2.packageoptions (configfile, category, package, set, unset)
VALUES
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', null, null, '{"DANE", "DTRACE", "FOP", "FORTRAN"}', '{"ALSA", "ASS", "CUPS", "DBUS", "DOXYGEN"}');

INSERT INTO poudrierec2.jobconfigs (id, name, poudriereconf, portstree, portset, jail)
VALUES
('209fc7b5-18c5-40e1-a205-4ae82790621e', 'Yes it''s a job configuration!', '97241b1e-9c04-4b58-9cdc-4c90eef35225', '4e6d2feb-2a99-4bed-8545-d5462c66ba0c', '11a4e47a-e778-4499-8ad3-4ad117fe0a2f', '0a62a787-c7c1-48bc-8ba0-90d7fbe9c098');

INSERT INTO poudrierec2.virtualmachines (azuuid, created)
VALUES
('0f2bb76c-b639-43d2-98d6-5c5dceb8ffe3', '2021-08-01 01:02:03Z'),
('94354c6f-1269-480e-9a81-543665e93c15', '2021-07-01 16:07:08Z');

INSERT INTO poudrierec2.schedules (jobconfig, runat)
VALUES
-- 03:42 daily
('209fc7b5-18c5-40e1-a205-4ae82790621e', '42 3 * * *');

INSERT INTO poudrierec2.jobruns (jobconfig, requested, virtualmachine)
VALUES
('209fc7b5-18c5-40e1-a205-4ae82790621e', '2021-08-05 00:00:00Z', '0f2bb76c-b639-43d2-98d6-5c5dceb8ffe3');

COMMIT;
