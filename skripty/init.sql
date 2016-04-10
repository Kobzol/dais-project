INSERT INTO "Role"(name)
VALUES('admin')
INSERT INTO "Role"(name)
VALUES('lector')
INSERT INTO "Role"(name)
VALUES('student')
GO

INSERT INTO "User"(roleId, name, surname)
VALUES (1, 'Jakub', 'Beránek')
INSERT INTO "User"(roleId, name, surname)
VALUES (2, 'Jan', 'Uèitel')
INSERT INTO "User"(roleId, name, surname)
VALUES (3, 'Lenka', 'Studentová')
GO