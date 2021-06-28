BEGIN WORK;
INSERT INTO poudrierec2.jails (name, version, architecture)
VALUES ('13_0-amd64', '13.0-RELEASE', 'amd64');

INSERT INTO poudrierec2.portstrees (name, method, url)
VALUES ('main', 'git+https', 'https://git.freebsd.org/ports.git');

INSERT INTO poudrierec2.portsets (name) VALUES ('server');

INSERT INTO poudrierec2.portset_members (portset, portname)
VALUES
('server', 'net/rclone'),
('server', 'security/sssd'),
('server', 'sysutils/tmux');

INSERT INTO poudrierec2.configfiles (id, deleted, name, configtype)
VALUES
('97241b1e-9c04-4b58-9cdc-4c90eef35225', false, 'Random poudriere.conf', 'poudriereconf');

INSERT INTO poudrierec2.configfiles (id, deleted, name, configtype, jail)
VALUES
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', false, 'Some make.conf', 'makeconf', '13_0-amd64');

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
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', 'DEFAULT_VERSIONS+', 'java=11 pgsql=13 php=8.0 python=3.8 python3=3.8 llvm=11 lua=5.4 samba=4.12 mono=6.8');

INSERT INTO poudrierec2.packageoptions (configfile, category, package, set, unset)
VALUES
('7557d8a8-bba5-4c99-ba6f-2ffbebb0be63', null, null, '{"DANE", "DTRACE", "FOP", "FORTRAN"}', '{"ALSA", "ASS", "CUPS", "DBUS", "DOXYGEN"}');

INSERT INTO poudrierec2.jobconfigs (id, title, portstree, portset, jail)
VALUES
('209fc7b5-18c5-40e1-a205-4ae82790621e', 'Yes it''s a job configuration!', 'main', 'server', '13_0-amd64');

INSERT INTO poudrierec2.jobconfigs_configfiles (id_jobconfigs, id_configfiles)
VALUES
('209fc7b5-18c5-40e1-a205-4ae82790621e', '7557d8a8-bba5-4c99-ba6f-2ffbebb0be63'),
('209fc7b5-18c5-40e1-a205-4ae82790621e', '97241b1e-9c04-4b58-9cdc-4c90eef35225');
COMMIT;