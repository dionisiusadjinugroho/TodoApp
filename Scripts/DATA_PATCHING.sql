INSERT INTO Users (Name,Username,PasswordHash,CreatedDate,UpdatedDate)
VALUES ('admin','admin','$2b$10$INai8BJbtjEieqHSRvmlVu6uEijvz7CCZ884mQBflenS7.dE.sIty', strftime('%Y-%m-%d %H:%M:%S', 'now'), strftime('%Y-%m-%d %H:%M:%S', 'now'));

WITH RECURSIVE generate_series(x) AS (
    SELECT 1
    UNION ALL
    SELECT x + 1
    FROM generate_series
    WHERE x < 1
)
INSERT INTO Activities (ActivitiesNo, UserID, Subject, Description, Status, CreatedDate, UpdatedDate)
SELECT 
    'AC-' || x,
    (SELECT UserID FROM Users LIMIT 1),
    'Subject ' || x,
    'Description ' || x,
    'Unmarked',
    strftime('%Y-%m-%d %H:%M:%S', 'now'),
    strftime('%Y-%m-%d %H:%M:%S', 'now')
FROM generate_series;