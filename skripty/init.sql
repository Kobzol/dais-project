INSERT INTO "Role"(name)
VALUES('admin')
INSERT INTO "Role"(name)
VALUES('lector')
INSERT INTO "Role"(name)
VALUES('student')
GO

INSERT INTO "User"(roleId, name, surname)
VALUES (1, 'Jakub', 'Ber�nek')
INSERT INTO "User"(roleId, name, surname)
VALUES (2, 'Jan', 'U�itel')
INSERT INTO "User"(roleId, name, surname)
VALUES (3, 'Lenka', 'Studentov�')
GO