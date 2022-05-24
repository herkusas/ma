INSERT INTO clients(
    client_id, client_name, access_token_lifetime)
VALUES ('admin', 'Admin', 300);

INSERT INTO api_scopes(
    name)
VALUES ('urn:masters:admin_api:robots:manage');

INSERT INTO api_scopes(
    name)
VALUES ('urn:masters:admin_api:resources:manage');

INSERT INTO client_secrets(
    client_id, value, type)
VALUES (1, 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'SharedSecret');

INSERT INTO client_scopes(
    client_id, scope_id)
VALUES (1, 1);

INSERT INTO client_scopes(
    client_id, scope_id)
VALUES (1, 2);

INSERT INTO api_resources(
    name)
VALUES ('https://localhost:7092');

INSERT INTO api_resource_scopes(
    api_resource_id, scope_id)
VALUES (1, 1);

INSERT INTO api_resource_scopes(
    api_resource_id, scope_id)
VALUES (1, 2);
