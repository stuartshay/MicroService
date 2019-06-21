CREATE TABLE public.test_data (id serial NOT NULL,"data" float8 NOT NULL,CONSTRAINT test_data_pkey PRIMARY KEY (id));
\COPY public.test_data FROM './sql_scripts/test_data_201904121704.csv' DELIMITER ',' CSV HEADER;
