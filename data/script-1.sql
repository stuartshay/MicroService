
CREATE TABLE public.test_data (
	id serial NOT NULL,
	"data" float8 NOT NULL,
	CONSTRAINT test_data_pkey PRIMARY KEY (id)
);


CREATE OR REPLACE FUNCTION public.get_data()
 RETURNS SETOF double precision
 LANGUAGE plpgsql
 STABLE
AS $function$
    BEGIN
      RETURN QUERY
      SELECT data
      FROM public.test_data;
    END
    $function$
;


select count(*) FROM public.test_data
2947.1106082201004

select * from public.get_data();

SELECT id, data FROM test_data


select "id", "data" from public.get_data();

select id, data from test_data;


